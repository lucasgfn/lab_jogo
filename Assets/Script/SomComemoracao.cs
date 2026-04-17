using UnityEngine;

/// <summary>
/// Toca um som de comemoracao (ex.: puzzle resolvido). Arraste o clip no Inspector no objeto do bequer.
/// Chame <see cref="TocarSeDisponivel"/> a partir de qualquer script.
/// </summary>
public class SomComemoracao : MonoBehaviour {
	public static SomComemoracao Instancia { get; private set; }

	[SerializeField]
	private AudioClip clipComemoracao;
	[SerializeField, Range(0f, 1f)]
	private float volume = 1f;

	private AudioSource fonte;

	private void Awake() {
		if(Instancia != null && Instancia != this) {
			Debug.LogWarning("Mais de um SomComemoracao na cena; mantendo o primeiro.");
			enabled = false;
			return;
		}
		Instancia = this;
		fonte = GetComponent<AudioSource>();
		if(fonte == null)
			fonte = gameObject.AddComponent<AudioSource>();
		fonte.playOnAwake = false;
		fonte.spatialBlend = 0f;
	}

	private void OnDestroy() {
		if(Instancia == this)
			Instancia = null;
	}

	public void Tocar() {
		if(clipComemoracao == null || fonte == null)
			return;
		fonte.PlayOneShot(clipComemoracao, volume);
	}

	public static void TocarSeDisponivel() {
		if(Instancia != null)
			Instancia.Tocar();
	}
}
