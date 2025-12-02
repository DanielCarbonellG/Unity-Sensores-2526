using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

using Gyroscope = UnityEngine.InputSystem.Gyroscope;
using Accelerometer = UnityEngine.InputSystem.Accelerometer;

public class WarriorController : MonoBehaviour
{
    [Header("Límites GPS (Tenerife)")]
    public float minLat = 28.0f; 
    public float maxLat = 29.0f;
    public float minLon = -17.0f;
    public float maxLon = -15.0f;
    
    [Header("Movimiento")]
    public float speedMultiplier = 5.0f;
    public float rotationSmoothing = 2.0f;
    
    // Variables de Estado
    private string statusMessage = "Iniciando...";
    private bool isInsideBounds = false;
    
    // Memoria de la última posición válida (para evitar parpadeos)
    private float lastValidLat = 0f;
    private float lastValidLon = 0f;
    private bool hasReceivedFirstData = false;

    void Start()
    {
        // 1. Habilitar sensores
        Input.compass.enabled = true;
        if (Accelerometer.current != null) InputSystem.EnableDevice(Accelerometer.current);
        if (Gyroscope.current != null) InputSystem.EnableDevice(Gyroscope.current);

        // 2. Arrancar GPS
        StartCoroutine(RestartLocationService());
    }

    // Función para el botón manual
    public void ManualRestart()
    {
        StopAllCoroutines();
        StartCoroutine(RestartLocationService());
    }

    IEnumerator RestartLocationService()
    {
        statusMessage = "Reiniciando servicio...";
        isInsideBounds = false; 

        // Parada limpia
        if (Input.location.status == LocationServiceStatus.Running)
        {
            Input.location.Stop();
        }
        yield return new WaitForSeconds(1.0f); // Espera vital para limpiar Unity Remote

        // Arranque
        statusMessage = "Solicitando GPS...";
        Input.location.Start(10f, 10f);

        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            statusMessage = $"Conectando... {maxWait}";
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (maxWait < 1 || Input.location.status == LocationServiceStatus.Failed)
        {
            statusMessage = "Fallo. Pulsa Reiniciar.";
        }
        else
        {
            statusMessage = "GPS Activo. Esperando datos...";
        }
    }

    void Update()
    {
        // Solo leemos si está CORRIENDO. Esto evita el error de consola.
        if (Input.location.status == LocationServiceStatus.Running)
        {
            float newLat = Input.location.lastData.latitude;
            float newLon = Input.location.lastData.longitude;

            // Filtro Anti-Cero (Unity Remote bug)
            if (newLat != 0 || newLon != 0)
            {
                lastValidLat = newLat;
                lastValidLon = newLon;
                hasReceivedFirstData = true;
                statusMessage = "SEÑAL OK";
            }
            else if (!hasReceivedFirstData)
            {
                statusMessage = "Recibiendo (0,0)...";
            }
        }

        CheckBounds(lastValidLat, lastValidLon);

        if (!isInsideBounds) return;

        // Orientación
        float heading = Input.compass.trueHeading;
        Quaternion targetRot = Quaternion.Euler(0, -heading, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSmoothing);

        // Movimiento
        if (Accelerometer.current != null)
        {
            Vector3 accel = Accelerometer.current.acceleration.ReadValue();
            float moveInput = -accel.z; 
            if (Mathf.Abs(moveInput) < 0.1f) moveInput = 0;
            transform.Translate(Vector3.forward * moveInput * speedMultiplier * Time.deltaTime);
        }
    }

    void CheckBounds(float lat, float lon)
    {
        if (!hasReceivedFirstData)
        {
            isInsideBounds = false;
            return;
        }

        if (lat >= minLat && lat <= maxLat && lon >= minLon && lon <= maxLon)
        {
            isInsideBounds = true;
        }
        else
        {
            isInsideBounds = false;
            statusMessage = $"FUERA: {lat:F2}, {lon:F2}";
        }
    }
    
    void OnGUI()
    {
        // Definimos un área en la parte DERECHA de la pantalla
        // Ancho de 350px, pegado al borde derecho
        float width = 350f;
        float padding = 20f;
        GUILayout.BeginArea(new Rect(Screen.width - width - padding, padding, width, Screen.height - padding));

        GUIStyle textStyle = new GUIStyle();
        textStyle.fontSize = 20; // Reducido de 40 a 20
        textStyle.normal.textColor = Color.white;
        textStyle.wordWrap = true; // Para que el texto no se salga si es largo
        
        GUILayout.Label($"Estado: {statusMessage}", textStyle);
        
        if (hasReceivedFirstData)
        {
            GUILayout.Label($"Lat: {lastValidLat}", textStyle);
            GUILayout.Label($"Lon: {lastValidLon}", textStyle);
        }
        
        // BOTÓN DE EMERGENCIA (Más pequeño)
        GUIStyle btnStyle = new GUIStyle("button");
        btnStyle.fontSize = 25; // Reducido de 50 a 25
        
        GUILayout.Space(10);
        if (GUILayout.Button("REINICIAR GPS", btnStyle, GUILayout.Height(60))) // Altura reducida
        {
            ManualRestart();
        }

        GUILayout.EndArea(); // Cerramos el área derecha
    }

}
