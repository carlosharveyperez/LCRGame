using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace LCRGame.Framework;

public class ValidationBase : ObservableObject, IDataErrorInfo
{
    public string Error { get; } = string.Empty;

    public string this[string columnName]
    {
        get
        {
            if (!ShowErrors) return string.Empty;
            return OnValidateProperty(columnName);
        }
    }

    public bool ShowErrors { get; set; }

    protected virtual string OnValidateProperty(string propertyName)
    {
        return string.Empty;
    }

    protected string GetFormattedErrors(string title, List<string> errors)
    {
        var listOfErrors = string.Join($"{Environment.NewLine}", errors);
        return @$"{title}{Environment.NewLine}{Environment.NewLine}{listOfErrors}";
    }
}