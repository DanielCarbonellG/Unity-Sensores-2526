using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.InputSystem.Controls;

// Resolvemos conflicto de nombres (Usamos el Nuevo Input System)
using Gyroscope = UnityEngine.InputSystem.Gyroscope;
using Accelerometer = UnityEngine.InputSystem.Accelerometer;

public class SensorManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI accelerometerText;
    public TextMeshProUGUI gyroscopeText;
    public TextMeshProUGUI gravityText;
    public TextMeshProUGUI attitudeText;
    public TextMeshProUGUI linearAccelText;
    public TextMeshProUGUI magneticText;
    // Eliminados: Presión, Proximidad y Pasos
    public TextMeshProUGUI statusText;

    void Update()
    {
        // INTENTO DE ACTIVACIÓN CONSTANTE
        // Vigilamos constantemente si Unity Remote conecta los sensores.
        CheckAndEnable(Accelerometer.current);
        CheckAndEnable(Gyroscope.current);
        CheckAndEnable(GravitySensor.current);
        CheckAndEnable(AttitudeSensor.current);
        CheckAndEnable(LinearAccelerationSensor.current);
        CheckAndEnable(MagneticFieldSensor.current);

        UpdateUI();
    }

    // Función auxiliar para encender sensores "perezosos"
    void CheckAndEnable(InputDevice device)
    {
        if (device != null && !device.enabled)
        {
            InputSystem.EnableDevice(device);
        }
    }

    void UpdateUI()
    {
        UpdateStatusText();

        // 1. Acelerómetro
        if (IsSensorActive(Accelerometer.current))
            accelerometerText.text = $"Accel: {Accelerometer.current.acceleration.ReadValue()}";
        else
            accelerometerText.text = "Accel: <color=red>No disp.</color>";

        // 2. Giroscopio
        if (IsSensorActive(Gyroscope.current))
            gyroscopeText.text = $"Gyro: {Gyroscope.current.angularVelocity.ReadValue()}";
        else
            gyroscopeText.text = "Gyro: <color=red>No disp.</color>";

        // 3. Gravedad
        if (IsSensorActive(GravitySensor.current))
            gravityText.text = $"Gravity: {GravitySensor.current.gravity.ReadValue()}";
        else
            gravityText.text = "Gravity: <color=red>No disp.</color>";

        // 4. Actitud (Orientación 3D)
        if (IsSensorActive(AttitudeSensor.current))
            attitudeText.text = $"Attitude: {AttitudeSensor.current.attitude.ReadValue()}";
        else
            attitudeText.text = "Attitude: <color=red>No disp.</color>";

        // 5. Lineal
        if (IsSensorActive(LinearAccelerationSensor.current))
            linearAccelText.text = $"Linear: {LinearAccelerationSensor.current.acceleration.ReadValue()}";
        else
            linearAccelText.text = "Linear: <color=red>No disp.</color>";

        // 6. Magnético (Brújula)
        if (IsSensorActive(MagneticFieldSensor.current))
            magneticText.text = $"Magnet: {MagneticFieldSensor.current.magneticField.ReadValue()} µT";
        else
            magneticText.text = "Magnet: <color=red>No disp.</color>";
    }
    
    bool IsSensorActive(InputDevice device)
    {
        return device != null && device.enabled;
    }
    
    void UpdateStatusText()
    {
        if (Accelerometer.current != null)
            statusText.text = "Sensores Activos (Datos recibidos)";
        else
            statusText.text = "Esperando Unity Remote...";
    }

    void OnDisable()
    {
        // Limpieza al salir
        if (Accelerometer.current != null) InputSystem.DisableDevice(Accelerometer.current);
        if (Gyroscope.current != null) InputSystem.DisableDevice(Gyroscope.current);
        if (GravitySensor.current != null) InputSystem.DisableDevice(GravitySensor.current);
        if (AttitudeSensor.current != null) InputSystem.DisableDevice(AttitudeSensor.current);
        if (MagneticFieldSensor.current != null) InputSystem.DisableDevice(MagneticFieldSensor.current);
    }
}