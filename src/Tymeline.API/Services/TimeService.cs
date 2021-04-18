using Microsoft.AspNetCore.Mvc;

public class TimeService :  ITimeService
{
    public int GetTime() => 5;
}