using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Utils.WebControls
{
  public abstract class InputControlAdapter
  {
    static Dictionary<Type, Type> adapterMap;
    static InputControlAdapter()
    {
      adapterMap = new Dictionary<Type, Type>();
      adapterMap.Add(typeof(TextBox), typeof(TextBoxInputControlAdapter)); 
      adapterMap.Add(typeof(DropDownList), typeof(DropDownListInputControlAdapter)); 
      adapterMap.Add(typeof(DateTimeSelector), typeof(DateTimeSelectorInputControlAdapter)); 
    }
    public static InputControlAdapter Create(Control inputControl)
    {
      if(!adapterMap.ContainsKey(inputControl.GetType()))
        throw new NotImplementedException(inputControl.GetType().ToString());

      Type adapterType = adapterMap[inputControl.GetType()];
      ConstructorInfo constructor = adapterType.GetConstructor(new Type[] { typeof(Control) });
      return constructor.Invoke(new object[] {inputControl}) as InputControlAdapter;

    }
    public static bool IsInputControl(Control ctrl)
    {
      return adapterMap.ContainsKey(ctrl.GetType());
    }
    protected  Control inputControl;
    public InputControlAdapter(Control inputControl)
    {
      this.inputControl = inputControl;
    }
    public abstract object Value { get; set; }
    public abstract string Text { get; }
    public void Clear()
    {
      Value = null;
    }
    #region Function For Get Value
    public string StringValue
    {
      get { return Value as string; }
    }
    public int IntegerValue
    {
      get { return (int)Value; }
    }
    public DateTime DateTimeValue
    {
      get { return (DateTime)Value; }
    }
    #endregion
  }
  public class TextBoxInputControlAdapter : InputControlAdapter
  {
    public TextBoxInputControlAdapter(Control inputControl) : base(inputControl) { }
    public TextBox InputControl
    {
      get { return inputControl as TextBox; } 
    }
    public override object Value
    {
      get
      {
        return InputControl.Text;
      }
      set
      {
        InputControl.Text = value + ""; 
      }
    }
    public override string Text
    {
      get
      {
        return InputControl.Text;
      }
    }
  }
  public class DropDownListInputControlAdapter : InputControlAdapter
  {
    public DropDownListInputControlAdapter(Control inputControl) : base(inputControl) { }
    public DropDownList InputControl
    {
      get { return inputControl as DropDownList; } 
    }
    public override object Value
    {
      get
      {
        return InputControl.SelectedValue;
      }
      set
      {
        InputControl.SelectedValue = value + ""; 
      }
    }
    public override string Text 
    {
      get
      {
        return InputControl.Text;
      }
    }
  }
  public class DateTimeSelectorInputControlAdapter : InputControlAdapter
  {
    public DateTimeSelectorInputControlAdapter(Control inputControl) : base(inputControl) { }
    public DateTimeSelector InputControl
    {
      get { return inputControl as DateTimeSelector; } 
    }
    public override object Value
    {
      get
      {
        return InputControl.Value;
      }
      set
      {
        InputControl.Value = value as DateTime?; 
      }
    }
    public override string Text 
    {
      get
      {
        return InputControl.Value.ToString();
      }
    }
  }
}
