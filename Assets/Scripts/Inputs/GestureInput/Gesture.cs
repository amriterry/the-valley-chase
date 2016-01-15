namespace App.TheValleyChase.Input.GestureInput {

    public class Gesture {
        private GestureType gestureType;

        public Gesture(GestureType type) {
            this.gestureType = type;
        }

        public GestureType GetGestureType() {
            return gestureType;
        }
    }

}