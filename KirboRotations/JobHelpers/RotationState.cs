namespace KirboRotations.JobHelpers
{

    internal enum OpenerState
    {
        PrePull,
        InOpener,
        OpenerFinished,
        FailedOpener
    }

    internal enum RotationType
    {
        None,
        Idle,
        Countdown,
        PrePull,
        InOpener,
        OpenerFinished,
        FailedOpener,
    }


}