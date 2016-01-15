using UnityEngine;

namespace App.TheValleyChase.Input.AccelerometerInput.Contracts {
    public interface IOnAccelerometerInput {
        void OnAccelerometerDetected(Vector3 fixedAcceleration);
    }
}
