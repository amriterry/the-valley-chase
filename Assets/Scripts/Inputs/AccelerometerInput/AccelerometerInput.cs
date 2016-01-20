using App.TheValleyChase.Input.AccelerometerInput.Contracts;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace App.TheValleyChase.Input.AccelerometerInput {

    public class AccelerometerInput : MonoBehaviour {

        private float sensitivity = 1f;

        private Quaternion calibrationQuaternion;
        private List<IOnAccelerometerInput> listeners;

        void Awake() {
            listeners = new List<IOnAccelerometerInput>();
        }

        void Start() {
            CalibrateAccelerometer();
        }

        void Update() {
#if MOBILE_INPUT
            InvokeListeners(FixAcceleration(UnityEngine.Input.acceleration));
#endif
        }

        private void InvokeListeners(Vector3 acceleration) {
            foreach (IOnAccelerometerInput listener in listeners) {
                listener.OnAccelerometerDetected(acceleration);
            }
        }

        public void RegisterListener(IOnAccelerometerInput listener) {
            listeners.Add(listener);
        }

        public void UnregisterListener(IOnAccelerometerInput listener) {
            listeners.Remove(listener);
        }

        public void CalibrateAccelerometer() {
            Vector3 accelerationSnapshot = UnityEngine.Input.acceleration;
            Quaternion rotateQuaternion = Quaternion.FromToRotation(new Vector3(0.0f, 0.0f, -1.0f), accelerationSnapshot);
            calibrationQuaternion = Quaternion.Inverse(rotateQuaternion);
        }

        public Vector3 FixAcceleration(Vector3 acceleration) {
            Vector3 fixedAcceleration = calibrationQuaternion * acceleration;
            return fixedAcceleration;
        }

        public float GetSensitivity() {
            return sensitivity;
        }
    }
}