using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.Core.ViewModels
{
    public class BasketSummeryViewModel
    {
        public int BasketCount { get; set; }
        public decimal BasketTotal { get; set; }

        public BasketSummeryViewModel()
        {

        }
        public BasketSummeryViewModel( int basketCount, decimal basketTotal) {
            this.BasketCount = basketCount;
            this.BasketTotal = basketTotal;
        }
    }
}
