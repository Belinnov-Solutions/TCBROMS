using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TCBROMS_Android_Webservice.Helpers
{
    public enum TableState
    {
        Free = 0,
        Occupied = 1,
        BillRequested = 2,
        Reserved = 3,
        ReservedAndOccupied = 4,
        iPodReceiptRequested = 5,
        DrinksOrdered = 6,
        RepeatDrinks = 7,
        CoffeeTime = 8,
        TableCleaning = 9,
        TableJoined = 10,
        TablePaid = 11,
        WaiterService = 12,
        BadFeedback = 13,
        PayAsYouGo = 14

    }
}