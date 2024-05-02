namespace DeepNestLib
{
  using System;

  public interface IMessageService
  {
    void DisplayMessage(string message);

    void DisplayMessage(Exception ex);

    void DisplayMessageBox(string text, string caption, MessageBoxIcon icon);

    void WriteToDebugConsole(string message);

    void WriteToDebugConsole(Exception ex);

    MessageBoxResult DisplayOkCancel(string text, string caption, MessageBoxIcon icon);

    MessageBoxResult DisplayYesNoCancel(string text, string caption, MessageBoxIcon icon);
  }
}