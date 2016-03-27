using UnityEngine;

public class Rotate : MonoBehaviour {
    public enum Axis { X, Y, Z}

    public Axis RotateAxis;
    public float Speed = 30f;

    void Update() {
        Vector3 rot = Vector3.zero;
        switch (RotateAxis) {
            case Axis.X: rot = Vector3.right; break;
            case Axis.Y: rot = Vector3.up; break;
            case Axis.Z: rot = Vector3.forward; break;
        }
        rot *= Speed * Time.deltaTime;

        transform.rotation *= Quaternion.Euler(rot);
    }
}
