using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Volume normalizado (0–1) do béquer, visual (Y / escala Y) e cor do líquido.
/// Cores: primeira ordem de derrame de cada tubo distinto define a mistura (média simples RGB, sem pesos por volume).
/// </summary>
public class ControleLiquidoBequer : MonoBehaviour {
	public static ControleLiquidoBequer Instancia { get; private set; }

	[SerializeField]
	private Transform liquido;
	[SerializeField]
	private float escalaYVazia = 0f;
	[SerializeField]
	private float escalaYCheia = 1f;
	[SerializeField]
	private float posicaoYVazia = 0.00210011f;
	[SerializeField]
	private float posicaoYCheia = 0.052f;
	[SerializeField, Range(0f, 1f)]
	private float volumeInicial = 0f;

	private float volume;
	private Material materialLiquidoInstancia;
	private Color corBaseInicial;
	private Color corLegadoInicial;
	private bool temEmission;
	private Color corEmissionInicial;
	private readonly HashSet<int> idsTubosJaDerramados = new HashSet<int>();
	private readonly List<Color> coresPorOrdemDeDerrame = new List<Color>();

	public float Volume => volume;
	public float EspacoRestante => Mathf.Max(0f, 1f - volume);

	private void Awake() {
		if(Instancia != null && Instancia != this) {
			Debug.LogWarning("Mais de um ControleLiquidoBequer na cena; mantendo o primeiro.");
			enabled = false;
			return;
		}
		Instancia = this;
		volume = Mathf.Clamp01(volumeInicial);
		if(liquido == null) {
			var encontrado = transform.Find("Beaker liquid");
			if(encontrado != null)
				liquido = encontrado;
			else if(transform.childCount > 0)
				liquido = transform.GetChild(0);
		}
		var rendererLiquido = liquido != null ? liquido.GetComponent<MeshRenderer>() : null;
		if(rendererLiquido != null && rendererLiquido.sharedMaterial != null) {
			materialLiquidoInstancia = new Material(rendererLiquido.sharedMaterial);
			rendererLiquido.material = materialLiquidoInstancia;
			if(materialLiquidoInstancia.HasProperty("_BaseColor"))
				corBaseInicial = materialLiquidoInstancia.GetColor("_BaseColor");
			else
				corBaseInicial = Color.white;
			if(materialLiquidoInstancia.HasProperty("_Color"))
				corLegadoInicial = materialLiquidoInstancia.GetColor("_Color");
			else
				corLegadoInicial = corBaseInicial;
			temEmission = materialLiquidoInstancia.HasProperty("_EmissionColor");
			corEmissionInicial = temEmission ? materialLiquidoInstancia.GetColor("_EmissionColor") : Color.black;
		}
	}

	private void OnDestroy() {
		if(Instancia == this)
			Instancia = null;
		if(materialLiquidoInstancia != null)
			Destroy(materialLiquidoInstancia);
	}

	private void Start() {
		AtualizarVisual();
	}

	/// <summary>
	/// Repoe o bequer para o estado inicial: volume igual a <see cref="volumeInicial"/>,
	/// limpa a ordem de mistura de cores e restaura as cores do material do liquido (como no Awake).
	/// Os tubos nao sao repostos automaticamente; chame <see cref="DerramarLiquido.ReporTubo"/> em cada um se precisar.
	/// </summary>
	public void Repor() {
		volume = Mathf.Clamp01(volumeInicial);
		idsTubosJaDerramados.Clear();
		coresPorOrdemDeDerrame.Clear();
		if(materialLiquidoInstancia != null) {
			if(materialLiquidoInstancia.HasProperty("_BaseColor"))
				materialLiquidoInstancia.SetColor("_BaseColor", corBaseInicial);
			if(materialLiquidoInstancia.HasProperty("_Color"))
				materialLiquidoInstancia.SetColor("_Color", corLegadoInicial);
			if(temEmission)
				materialLiquidoInstancia.SetColor("_EmissionColor", corEmissionInicial);
		}
		AtualizarVisual();
	}

	/// <param name="deltaNormalizado">Volume a acrescentar no béquer (0–1).</param>
	/// <param name="corLiquidoTubo">Cor do líquido deste tubo (material do mesh do líquido).</param>
	/// <param name="idInstanciaTubo">Identificador estável do tubo (ex.: GetInstanceID do DerramarLiquido).</param>
	public void AdicionarVolume(float deltaNormalizado, Color corLiquidoTubo, int idInstanciaTubo) {
		if(deltaNormalizado <= 0f)
			return;
		volume = Mathf.Clamp01(volume + deltaNormalizado);
		if(idsTubosJaDerramados.Add(idInstanciaTubo))
			coresPorOrdemDeDerrame.Add(corLiquidoTubo);
		AtualizarVisual();
		AplicarCorMisturada();
	}

	private void AplicarCorMisturada() {
		if(materialLiquidoInstancia == null || coresPorOrdemDeDerrame.Count == 0)
			return;
		Color media = Color.black;
		foreach(Color c in coresPorOrdemDeDerrame)
			media += c;
		float n = coresPorOrdemDeDerrame.Count;
		media.r /= n;
		media.g /= n;
		media.b /= n;
		media.a = 1f;
		if(materialLiquidoInstancia.HasProperty("_BaseColor"))
			materialLiquidoInstancia.SetColor("_BaseColor", media);
		if(materialLiquidoInstancia.HasProperty("_Color"))
			materialLiquidoInstancia.SetColor("_Color", media);
	}

	private void AtualizarVisual() {
		if(liquido == null)
			return;
		float t = Mathf.Clamp01(volume);
		Vector3 lp = liquido.localPosition;
		lp.y = Mathf.Lerp(posicaoYVazia, posicaoYCheia, t);
		liquido.localPosition = lp;
		Vector3 ls = liquido.localScale;
		ls.y = Mathf.Lerp(escalaYVazia, escalaYCheia, t);
		liquido.localScale = ls;
		bool visivel = t > 0.001f;
		if(liquido.gameObject.activeSelf != visivel)
			liquido.gameObject.SetActive(visivel);
	}
}
