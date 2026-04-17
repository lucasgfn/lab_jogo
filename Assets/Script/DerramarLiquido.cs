using UnityEngine;

/// <summary>
/// Enquanto o tubo esta inclinado na zona de derramar, o liquido do tubo diminui (Y e escala Y)
/// e o bequer recebe volume proporcional a proporcaoCapacidadeBequer.
/// A soma das proporcoes dos tres tubos nao deve ultrapassar 1 (um bequer cheio).
/// A cor no bequer segue a ordem do primeiro derrame de cada tubo: media simples das cores (sem peso por volume).
/// Som de derrame: loop enquanto houver derrame efetivo; para ao sair da zona, endireitar, esvaziar ou encher o bequer.
/// </summary>
public class DerramarLiquido : MonoBehaviour {
	[SerializeField]
	private float anguloMin = 100f;
	[Tooltip("Volume normalizado (0-1) retirado do tubo por segundo ao derramar.")]
	[SerializeField]
	private float taxaDerramamento = 0.35f;
	[Tooltip("Fracao do volume maximo do bequer (0-1) que este tubo pode acrescentar quando esvaziado por completo. Tres tubos com 1/3 cada somam 1.")]
	[SerializeField, Range(0.001f, 1f)]
	private float proporcaoCapacidadeBequer = 0.33333334f;
	[SerializeField]
	private ControleLiquidoBequer bequer;
	[SerializeField]
	private Transform liquidoNoTubo;
	[SerializeField]
	private AudioClip clipSomDerrame;
	[SerializeField]
	private float tuboEscalaYCheio = 0.75f;
	[SerializeField]
	private float tuboEscalaYVazio = 0.02f;
	[SerializeField]
	private float tuboPosYCheio = 0.0036000013f;
	[SerializeField]
	private float tuboPosYVazio = -0.035f;
	[SerializeField, Range(0f, 1f)]
	private float liquidoInicialNoTubo = 1f;

	private bool inZonaDerramar;
	private float liquidoRestanteNoTubo;
	private AudioSource fonteAudio;

	private void Awake() {
		liquidoRestanteNoTubo = Mathf.Clamp01(liquidoInicialNoTubo);
		if(liquidoNoTubo == null && transform.childCount > 0)
			liquidoNoTubo = transform.GetChild(0);
		fonteAudio = GetComponent<AudioSource>();
		if(fonteAudio == null)
			fonteAudio = gameObject.AddComponent<AudioSource>();
		fonteAudio.playOnAwake = false;
		fonteAudio.loop = true;
		fonteAudio.spatialBlend = 0f;
	}

	private void OnDisable() {
		PararSomDerrame();
	}

	private void Start() {
		if(bequer == null)
			bequer = ControleLiquidoBequer.Instancia;
		AtualizarVisualTubo();
	}

	/// <summary>
	/// Volta o nivel do liquido do tubo ao valor de <see cref="liquidoInicialNoTubo"/> (visual Y / escala Y).
	/// Use junto com <see cref="ControleLiquidoBequer.Repor"/> para reiniciar o puzzle completo.
	/// </summary>
	public void ReporTubo() {
		liquidoRestanteNoTubo = Mathf.Clamp01(liquidoInicialNoTubo);
		AtualizarVisualTubo();
	}

	private void Update() {
		if(bequer == null)
			bequer = ControleLiquidoBequer.Instancia;

		bool derramando = false;
		if(inZonaDerramar && liquidoRestanteNoTubo > 0.0001f && bequer != null) {
			float angulo = Vector3.Angle(transform.up, Vector3.up);
			if(angulo > anguloMin) {
				float maxDfTubo = liquidoRestanteNoTubo;
				float quota = Mathf.Max(proporcaoCapacidadeBequer, 0.0001f);
				float maxDfTuboPeloBequer = bequer.EspacoRestante / quota;
				float df = Mathf.Min(taxaDerramamento * Time.deltaTime, maxDfTubo, maxDfTuboPeloBequer);
				derramando = df > 0f;
				if(derramando) {
					liquidoRestanteNoTubo -= df;
					Color cor = ObterCorDoLiquidoNoTubo();
					bequer.AdicionarVolume(quota * df, cor, GetInstanceID());
					AtualizarVisualTubo();
				}
			}
		}

		AtualizarSomDerrame(derramando);
	}

	private void AtualizarSomDerrame(bool derramando) {
		if(clipSomDerrame == null || fonteAudio == null)
			return;
		if(derramando) {
			if(fonteAudio.clip != clipSomDerrame || !fonteAudio.isPlaying) {
				fonteAudio.clip = clipSomDerrame;
				fonteAudio.loop = true;
				fonteAudio.Play();
			}
		} else {
			if(fonteAudio.isPlaying && fonteAudio.clip == clipSomDerrame)
				fonteAudio.Stop();
		}
	}

	private void PararSomDerrame() {
		if(fonteAudio != null && fonteAudio.isPlaying && fonteAudio.clip == clipSomDerrame)
			fonteAudio.Stop();
	}

	private static Color ObterCorDoMaterial(Material mat) {
		if(mat == null)
			return Color.white;
		if(mat.HasProperty("_BaseColor"))
			return mat.GetColor("_BaseColor");
		if(mat.HasProperty("_Color"))
			return mat.GetColor("_Color");
		return mat.color;
	}

	private Color ObterCorDoLiquidoNoTubo() {
		if(liquidoNoTubo == null)
			return Color.white;
		var mr = liquidoNoTubo.GetComponent<MeshRenderer>();
		if(mr == null)
			return Color.white;
		return ObterCorDoMaterial(mr.sharedMaterial);
	}

	private void AtualizarVisualTubo() {
		if(liquidoNoTubo == null)
			return;
		float t = Mathf.Clamp01(liquidoRestanteNoTubo);
		Vector3 lp = liquidoNoTubo.localPosition;
		lp.y = Mathf.Lerp(tuboPosYVazio, tuboPosYCheio, t);
		liquidoNoTubo.localPosition = lp;
		Vector3 ls = liquidoNoTubo.localScale;
		ls.y = Mathf.Lerp(tuboEscalaYVazio, tuboEscalaYCheio, t);
		liquidoNoTubo.localScale = ls;
	}

	private void OnTriggerEnter(Collider other) {
		if(other.CompareTag("ZonaDerramar")) {
			var outline = gameObject.GetComponent<Outline>();
			if(outline != null)
				outline.OutlineWidth = 5f;
			inZonaDerramar = true;
		}
	}

	private void OnTriggerExit(Collider other) {
		if(other.CompareTag("ZonaDerramar")) {
			var outline = gameObject.GetComponent<Outline>();
			if(outline != null)
				outline.OutlineWidth = 0f;
			inZonaDerramar = false;
		}
	}
}
