namespace DeepNestLib
{
  using System;

  public interface IMessageService
  {
    void DisplayMessage(string message);

    void DisplayMessage(Exception ex);

    void DisplayMessageBox(string text, string caption, MessageBoxIcon icon);

    void StartRecord();

    void StopRecord();

    /// <summary>
    /// Print the record to the console or other output. Just pass the action from the message service.
    /// </summary>
    /// <param name="action"></param>
    void PrintRecord(Action<string> action);

    void SaveRecord(string path);

    void WriteToDebugConsole(string message);

    void WriteToDebugConsole(Exception ex);

    MessageBoxResult DisplayOkCancel(string text, string caption, MessageBoxIcon icon);

    MessageBoxResult DisplayYesNoCancel(string text, string caption, MessageBoxIcon icon);
  }
}