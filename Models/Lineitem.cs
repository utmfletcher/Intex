using System;
using System.Collections.Generic;

namespace Intex.Models;

public partial class Lineitem
{
    public long? TransactionId { get; set; }

    public long? ProductId { get; set; }

    public long? Qty { get; set; }

    public long? Rating { get; set; }
}
