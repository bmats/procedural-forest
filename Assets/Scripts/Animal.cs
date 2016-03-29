using UnityEngine;
using System.Linq;

/// A critter which moves around randomly. When the player approaches it, it stands still.
/// When the player gets too close, it spooks and starts running away, knocking over things in its path.
[RequireComponent(typeof(Rigidbody))]
class Animal : MonoBehaviour {
    public float Speed = 10f;
    public float NoticeDistance = 15f;
    public float SpookDistance = 7f;
    public float SpookLength = 8f;
    public float SpookSpeed = 12f;
    /// The tags of objects that I will knock over.
    public string[] SpookCollisionTags = null;

    private ForestBuilder _builder;
    private Transform _controller;
    private Rigidbody _rigidbody;
    private bool _spooked = false;
    private Vector3 _direction;
    private float _directionChangeTime = -1f;

    void Start() {
        _builder = GameManager.Instance.Builder;
        _controller = GameManager.Instance.Controller;
        _rigidbody = GetComponent<Rigidbody>();
    }

    void Update() {
        // Change direction if it's time
        if (Time.time >= _directionChangeTime) {
            if (_spooked) {
                // Stop if spooked
                _direction = Vector3.zero;
                _spooked = false;
            } else {
                // Choose a random direction to move in
                _direction = Random.insideUnitSphere;
                _direction.y = 0;
            }

            _directionChangeTime = Time.time + Random.Range(1f, 4f);
        }

        float sqrDistance = (transform.position - _controller.position).sqrMagnitude;
        if (sqrDistance < SpookDistance * SpookDistance) {
            // In spook range, start running in the opposite direction
            _spooked = true;
            _direction = transform.position - _controller.position;
            _direction.Normalize();
            _directionChangeTime = Time.time + SpookLength; // run for SpookLength seconds
            Move();
        } else if (!_spooked && sqrDistance < NoticeDistance * NoticeDistance) {
            // In notice range, don't move
        } else {
            Move();
        }

        // Rotate to ground
        transform.rotation = Quaternion.FromToRotation(
            Vector3.up,
            _builder.GetGroundNormal(transform.position.x, transform.position.z));
    }

    private void Move() {
        if (_direction.sqrMagnitude > 0) { // don't stop abruptly
            _rigidbody.AddForce(
                _direction * (_spooked ? SpookSpeed : Speed) * Time.deltaTime,
                ForceMode.VelocityChange);
        }
    }

    void OnCollisionEnter(Collision collision) {
        // Knock down trees and rocks when I am spooked
        if (_spooked && SpookCollisionTags.Contains(collision.gameObject.tag)) {
            var chain = collision.gameObject.AddComponent<RigidbodyChainReaction>();
            chain.Tags = SpookCollisionTags;
        }
    }
}
