using UnityEngine;

namespace Slacken.Bases.SO
{
    [CreateAssetMenu(fileName = "Message Event", menuName = "Slacken/Events/Message")]
    public class MessageEvent : ScriptableObject
    {
        public delegate void OnNewMessage(string message);
        public event OnNewMessage onNewMessageReceived;

        public void SendMessage(string message)
        {
            // Trigger that a new message has been called
            onNewMessageReceived?.Invoke(message);
        }

        private void OnDestroy()
        {
            // Reset the events if the object gets destroyed
            onNewMessageReceived = null;
        }
    }
}
