using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour
{
	[SerializeField]
	private float m_updateInterval = 0.5f;

	private float m_accum;
	private int m_frames;
	private float m_timeleft;
	private float m_fps;
	private Text m_Text;

	private void Awake()
	{
		m_Text = GetComponent<Text>();
	}

	private void Update()
	{
		m_timeleft -= Time.deltaTime;
		m_accum += Time.timeScale / Time.deltaTime;
		m_frames++;

		if ( 0 < m_timeleft ) return;

		m_fps = m_accum / m_frames;
		m_Text.text = "FPS: " + m_fps.ToString( "f1" );
		m_timeleft = m_updateInterval;
		m_accum = 0;
		m_frames = 0;
	}
}
