using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using App.TheValleyChase.Input.GestureInput.Contracts;

namespace App.TheValleyChase.Input.GestureInput {

    public class GestureInput : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler {

        public float maxPointDownTime = 1f;
        public float minSwipeDist = 50f;

        private Vector2 origin;
        private Vector2 direction;
        private float timer;

        private List<IOnGestureInput> gestureListeners;

        void Awake() {
            gestureListeners = new List<IOnGestureInput>();
        }

        public void OnPointerDown(PointerEventData data) {
            origin = data.position;
            direction = Vector2.zero;
            timer = 0;
        }

        public void OnDrag(PointerEventData data) {
            direction = (data.position - origin);
            timer += Time.deltaTime;
        }

        public void OnPointerUp(PointerEventData data) {
            origin = Vector2.zero;

            if (timer <= maxPointDownTime && direction.magnitude > 50f) {
                //player.HandleSwipe (direction.normalized);

                HandleRawGesture(direction.normalized);
            }
        }

        private void HandleRawGesture(Vector2 swipeDirection) {
            Gesture gesture;

            if ((Mathf.Abs(swipeDirection.x) - Mathf.Abs(swipeDirection.y)) > 0) {
                // Swipe to left or right is greater than vertical swipe.

                if (swipeDirection.x > 0) {
                    gesture = new Gesture(GestureType.SWIPE_RIGHT);
                } else if (swipeDirection.x < 0) {
                    gesture = new Gesture(GestureType.SWIPE_LEFT);
                } else {
                    return;
                }
            } else {
                // Swipe to up or down is greater than horizontal swipe.

                if (swipeDirection.y > 0) {
                    gesture = new Gesture(GestureType.SWIPE_UP);
                } else if (swipeDirection.y < 0) {
                    gesture = new Gesture(GestureType.SWIPE_DOWN);
                } else {
                    return;
                }
            }

            InvokeListeners(gesture);
        }

        /**
         *	Register a gesture input listener. 
         **/
        public void RegisterListener(IOnGestureInput listener) {
            gestureListeners.Add(listener);
        }

        /**
         *	Invokes all gesture listeners with respective gesture input. 
         **/
        private void InvokeListeners(Gesture gesture) {
            foreach (IOnGestureInput listener in gestureListeners) {
                listener.OnSwipe(gesture);
            }
        }
    }

}
