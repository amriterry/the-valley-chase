using UnityEngine;
using System.Collections;
using App.TheValleyChase.Framework;

namespace App.TheValleyChase.Player {

    public class PlayerCollision : MonoBehaviour {

        void OnCollisionEnter(Collision col) {
            if (col.gameObject.tag == TagsContainer.Obstacle) {
                Die();
            }
        }

        private void Die() {
            Debug.Log("Die");
            gameObject.GetComponent<PlayerMovement>().StopMovement();
        }
    }
}