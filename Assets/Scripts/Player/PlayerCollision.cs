using UnityEngine;
using App.TheValleyChase.Framework;

namespace App.TheValleyChase.Player {

    public class PlayerCollision : MonoBehaviour {

        private PlayerMovement movementController;

        void Awake() {
            movementController = GetComponent<PlayerMovement>();
        }

        void OnCollisionEnter(Collision col) {
            if (col.gameObject.tag == TagsContainer.Obstacle) {
                DieConditionally(col);
            }
        }

        void OnTriggerEnter(Collider col) {
            if (col.gameObject.tag == TagsContainer.TurnTrigger) {
                movementController.EnableTurning(col.gameObject.GetComponent<TurnTrigger>());
            }
        }

        void OnTriggerExit(Collider col) {
            if(col.gameObject.tag == TagsContainer.TurnTrigger) {
                movementController.DisableTurning();
            }
        }

        private void DieConditionally(Collision col) {
            Debug.Log("Die");

            //gameObject.GetComponent<PlayerMovement>().StopMovement();
        }
    }
}