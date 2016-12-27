
namespace WPWebSockets.Events
{
    public class JSONMultiFrameEventArgs : JSONFrameEventArgs
    {
        public bool IsLastFrame { get; private set; }

        public JSONMultiFrameEventArgs(string json, bool isLastFrame)
            : base(json)
        {
            IsLastFrame = isLastFrame;
        }
    }
}
