using UnityEngine;
using UnityEngine.UI;

public class FramerateCounter : MonoBehaviour
{
    /// <summary>
    /// External access to fps
    /// </summary>
    public int FPS
    {
        get { return (int)m_currentFps; }
    }

    //FPS Related settings
    #region FPS Variables
    const string cFormat = "FPS {0} / {1:0.00}ms";
    const float cMeasurePeriod = 1f;
    private float m_currentFps;
    private float m_currentMs;
    private float m_fpsAccumulator = 0;
    private float m_fpsNextPeriod = 0;
    public Text m_fpsText;
    #endregion

    private void Start()
    {
        m_fpsNextPeriod = Time.realtimeSinceStartup + cMeasurePeriod;
    }

    private void Update()
    {
        // measure average frames per second
        m_fpsAccumulator++;
        if (Time.realtimeSinceStartup > m_fpsNextPeriod)
        {
            m_currentFps = m_fpsAccumulator / cMeasurePeriod;
            m_currentMs = 1000f / m_currentFps;
            m_fpsAccumulator = 0f;
            m_fpsNextPeriod = Time.realtimeSinceStartup + cMeasurePeriod;
            if (m_fpsText != null)
            {
                m_fpsText.text = string.Format(cFormat, m_currentFps, m_currentMs);
            }
        }
    }
}
