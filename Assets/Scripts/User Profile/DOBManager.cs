using System;
using System.Globalization;
using TMPro;
using UnityEngine;

public class DOBManager : MonoBehaviour
{
    public TMP_Dropdown dayDropdown;
    public TMP_Dropdown monthDropdown;
    public TMP_Dropdown yearDropdown;

    private void Start()
    {
        PopulateMonthDropdown();
        PopulateYearDropdown();

        monthDropdown.onValueChanged.AddListener(delegate { UpdateDayDropdown(); });
        yearDropdown.onValueChanged.AddListener(delegate { UpdateDayDropdown(); });

        UpdateDayDropdown(); // Initial population of day dropdown
    }

    private void PopulateMonthDropdown()
    {
        monthDropdown.ClearOptions();
        string[] months = DateTimeFormatInfo.CurrentInfo.MonthNames;
        foreach (string month in months)
        {
            if (!string.IsNullOrEmpty(month))
            {
                monthDropdown.options.Add(new TMP_Dropdown.OptionData(month));
            }
        }
        monthDropdown.RefreshShownValue();
    }

    private void PopulateYearDropdown()
    {
        yearDropdown.ClearOptions();
        int currentYear = DateTime.Now.Year;
        for (int year = currentYear; year >= 1900; year--)
        {
            yearDropdown.options.Add(new TMP_Dropdown.OptionData(year.ToString()));
        }
        yearDropdown.RefreshShownValue();
    }

    private void UpdateDayDropdown()
    {
        if (monthDropdown.options.Count == 0 || yearDropdown.options.Count == 0) return;

        int selectedMonth = monthDropdown.value + 1;
        int selectedYear = int.Parse(yearDropdown.options[yearDropdown.value].text);

        int daysInMonth = DateTime.DaysInMonth(selectedYear, selectedMonth);

        dayDropdown.ClearOptions();
        for (int day = 1; day <= daysInMonth; day++)
        {
            dayDropdown.options.Add(new TMP_Dropdown.OptionData(day.ToString()));
        }
        dayDropdown.RefreshShownValue();
    }

    public string GetSelectedDate()
    {
        if (dayDropdown.options.Count == 0 || monthDropdown.options.Count == 0 || yearDropdown.options.Count == 0)
        {
            return null;
        }

        int day = int.Parse(dayDropdown.options[dayDropdown.value].text);
        int month = monthDropdown.value + 1;
        int year = int.Parse(yearDropdown.options[yearDropdown.value].text);

        return new DateTime(year, month, day).ToString("yyyy-MM-dd");
    }

    public void ClearDateSelection()
    {
        dayDropdown.ClearOptions();
        monthDropdown.ClearOptions();
        yearDropdown.ClearOptions();
        PopulateMonthDropdown();
        PopulateYearDropdown();
        UpdateDayDropdown();
    }
}
