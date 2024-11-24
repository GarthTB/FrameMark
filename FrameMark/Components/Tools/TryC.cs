namespace FrameMark.Components.Tools
{
    /// <summary> 为简化代码，将TryCatch块封装成一个类 </summary>
    internal static class TryC
    {
        /// <summary> Try一个Action，如果出错，则显示MessageBox </summary>
        /// <returns> 有异常则false，无异常则true </returns>
        public static bool Do(string name, Action action)
        {
            try
            {
                action();
                return true;
            }
            catch (Exception ex)
            {
                MsgB.OkErr($"{name}出错：\n{ex.Message}", "错误");
                return false;
            }
        }
    }
}
