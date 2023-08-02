namespace KirboRotations.JobHelpers
{
    internal class RotationState
    {
        internal enum OpenerState
        {
            _,
            PrePull,
            InOpener,
            OpenerFinished,
            FailedOpener
        }

    }
}
