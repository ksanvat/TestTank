namespace PrjCore.System.AsyncTask {
    public enum PAsyncTaskStatus {
        NotStarted,
            
        Running,
        CancelRequested,
            
        Success,
        Cancelled,
        Failed,
            
        UnexpectedError,
        Disposed
    }
}