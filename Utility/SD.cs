using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Utility
{
    public static class SD
    {
        public const string ManagerRole = "Manager";
        public const string FontDeskRole = "Font";
        public const string KitchenRole = "Kitchen";
        public const string CustomerRole = "Customer";

        public const string ShoppingCard = "ShoppingCard";

        public const string StatusSubmitted = "Submitted";
        public const string StatusInProcess = "Being Prepared";
        public const string StatusReady = "Ready for Pickup";
        public const string StatusCompleted = "Completed";
        public const string StatusCancelled = "Cancelled";
        public const string StatusRefunded = "Refunded";

        public const string PaymentStatusPending = "Pending";
        public const string PaymentStatusApproved = "Approved";
        public const string PaymentStatusRejeted = "Rejected";

        public static T toObject<T>(string json)
        {
            return (T)JsonConvert.DeserializeObject(json);
        }
    }
}
