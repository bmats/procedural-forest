using UnityEngine;
using System.Linq;

/// Add a rigidbody to myself and to any objects which I hit matching certain tags.
class RigidbodyChainReaction : MonoBehaviour {
    public string[] Tags = null;

    void Start() {
        if (gameObject.GetComponent<Rigidbody>() == null) {
            gameObject.AddComponent<Rigidbody>();
        }
    }

    void OnCollisionEnter(Collision collision) {
        if (Tags.Contains(collision.gameObject.tag) &&
                collision.gameObject.GetComponent<RigidbodyChainReaction>() == null) {
            var chain = collision.gameObject.AddComponent<RigidbodyChainReaction>();
            chain.Tags = Tags;
        }
    }
}
