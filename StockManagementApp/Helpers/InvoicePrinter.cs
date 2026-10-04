using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Forms;

namespace StockManagementApp.Helpers;

public class InvoiceItem
{
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal GstPercent { get; set; }
    public decimal Discount { get; set; }
    public decimal Total => Math.Round((Quantity * UnitPrice) + ((Quantity * UnitPrice) * GstPercent / 100) - Discount, 2);
}

public class InvoiceData
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; } = DateTime.Now;
    public string TransactionType { get; set; } = string.Empty;
    public string PartyName { get; set; } = string.Empty;
    public string PartyAddress { get; set; } = string.Empty;
    public string PartyMobile { get; set; } = string.Empty;
    public string PartyGstNumber { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public List<InvoiceItem> Items { get; } = new();
    public string InvoiceType { get; set; } = "A4";

    public decimal SubTotal    => Math.Round(Items.Sum(i => i.Quantity * i.UnitPrice), 2);
    public decimal TotalGst    => Math.Round(Items.Sum(i => (i.Quantity * i.UnitPrice) * i.GstPercent / 100), 2);
    public decimal TotalDiscount => Math.Round(Items.Sum(i => i.Discount), 2);
    public decimal GrandTotal  => Math.Round(Items.Sum(i => i.Total), 2);
}

public static class InvoicePrinter
{
    public static void ShowPrintPreview(InvoiceData invoice)
    {
        using var printDocument = CreatePrintDocument(invoice);
        using var preview = new PrintPreviewDialog { Document = printDocument, Width = 900, Height = 700 };
        preview.ShowDialog();
    }

    public static void PrintInvoice(InvoiceData invoice, string? printerName = null)
    {
        using var printDocument = CreatePrintDocument(invoice);

        if (!string.IsNullOrWhiteSpace(printerName))
        {
            printDocument.PrinterSettings.PrinterName = printerName;
        }
        else
        {
            using var printDialog = new PrintDialog
            {
                Document = printDocument,
                AllowSomePages = false,
                AllowSelection = false,
                UseEXDialog = true
            };
            if (printDialog.ShowDialog() != DialogResult.OK)
                return;
        }

        printDocument.Print();
    }

    private static PrintDocument CreatePrintDocument(InvoiceData invoice)
    {
        var printDocument = new PrintDocument();
        printDocument.DocumentName = "SS Traders - " + invoice.TransactionType + " Invoice";
        printDocument.PrintPage += (sender, e) => RenderInvoicePage(e.Graphics!, e.MarginBounds, invoice);

        if (invoice.InvoiceType == "Thermal")
        {
            printDocument.DefaultPageSettings.PaperSize  = new PaperSize("Thermal", 228, 1000);
            printDocument.DefaultPageSettings.Margins    = new Margins(10, 10, 10, 10);
        }
        else
        {
            printDocument.DefaultPageSettings.PaperSize  = new PaperSize("A4", 827, 1169);
            printDocument.DefaultPageSettings.Margins    = new Margins(50, 50, 50, 50);
        }

        return printDocument;
    }

    private static string ResolveLogoPath()
    {
        var appDir = Path.GetDirectoryName(
            System.Reflection.Assembly.GetExecutingAssembly().Location) ?? ".";
        return Path.Combine(appDir, "Resources", "logo.png");
    }

    private static void RenderInvoicePage(Graphics g, Rectangle bounds, InvoiceData invoice)
    {
        g.SmoothingMode     = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        var navy   = Color.FromArgb(15,  52,  96);
        var teal   = Color.FromArgb(14, 165, 233);
        var rowAlt = Color.FromArgb(241, 248, 255);
        var dark   = Color.FromArgb(30,  30,  30);
        var gray   = Color.FromArgb(100, 100, 100);

        using var navyBrush  = new SolidBrush(navy);
        using var darkBrush  = new SolidBrush(dark);
        using var grayBrush  = new SolidBrush(gray);
        using var whiteBrush = new SolidBrush(Color.White);
        using var tealPen    = new Pen(teal, 3);
        using var navyPen    = new Pen(navy, 1);

        using var headFont   = new Font("Segoe UI", 10, FontStyle.Bold);
        using var normalFont = new Font("Segoe UI", 10);
        using var addrFont   = new Font("Segoe UI", 10, FontStyle.Bold);
        using var infoFont   = new Font("Segoe UI", 10, FontStyle.Bold);
        using var totalFont  = new Font("Segoe UI", 11, FontStyle.Bold);
        using var smallFont  = new Font("Segoe UI",  8, FontStyle.Italic);

        // ── HEADER ──────────────────────────────────────────────────────
        // Logo at the absolute LEFT edge of the paper (x=0, y=0).
        // Address starts from where the CIRCLE part of the logo ends
        // (≈63 % of the logo height) — NOT after the full logo image.
        // Badge + Invoice No + Date on the right side.
        // ────────────────────────────────────────────────────────────────
        int logoH = 240;
        int logoW = 240;

        var logoPath = ResolveLogoPath();
        if (File.Exists(logoPath))
        {
            using var img = Image.FromFile(logoPath);
            logoW = (int)Math.Round(logoH * (double)img.Width / img.Height);
            // Draw from absolute page corner (x=0, y=0) — outside the margin
            g.DrawImage(img, 0, 0, logoW, logoH);
        }
        else
        {
            g.FillRectangle(navyBrush, new Rectangle(0, 0, logoW, logoH));
            using var fb = new Font("Segoe UI", 34, FontStyle.Bold);
            var fs = g.MeasureString("SS", fb);
            g.DrawString("SS", fb, whiteBrush,
                (logoW - (int)fs.Width) / 2,
                (logoH - (int)fs.Height) / 2);
        }

        // The visible circle ends at ≈63 % of the logo height.
        // Address is placed right there, left-aligned with the content margin.
        int circleEndY = (int)(logoH * 0.83);   // ≈ 151
        int addrY = circleEndY + 4;
        g.DrawString("Gandhinagar, Jangipur, Ghazipur",
                     addrFont, darkBrush, bounds.Left, addrY);
        addrY += 18;
        g.DrawString("Uttar Pradesh  •  Pin 233305",
                     addrFont, navyBrush, bounds.Left, addrY);
        addrY += 22;

        // Invoice type badge — top-right, pushed 55 units down from paper edge
        var badgeText = invoice.TransactionType.ToUpper() + " INVOICE";
        var bsz  = g.MeasureString(badgeText, headFont);
        var bRec = new RectangleF(bounds.Right - bsz.Width - 20, 55,
                                  bsz.Width + 18, bsz.Height + 8);
        g.FillRectangle(navyBrush, bRec);
        g.DrawString(badgeText, headFont, whiteBrush, bRec.Left + 9, bRec.Top + 4);

        // Invoice No and Date — one line each, bold, below badge
        int infoX = bounds.Right - 320;
        int infoY = (int)bRec.Bottom + 16;
        g.DrawString($"Invoice No :  {invoice.InvoiceNumber}",
                     infoFont, navyBrush, infoX, infoY);
        infoY += 24;
        g.DrawString($"Date :  {invoice.InvoiceDate:dd-MM-yyyy  HH:mm}",
                     infoFont, navyBrush, infoX, infoY);
        infoY += 24;

        // Separator comes after all header content AND after the logo bottom
        var y = Math.Max(logoH + 8, Math.Max(addrY, infoY + 6)) + 10;

        // Double rule
        g.DrawLine(tealPen, bounds.Left, y, bounds.Right, y);
        using (var lp = new Pen(Color.FromArgb(180, 210, 235), 1))
            g.DrawLine(lp, bounds.Left, y + 4, bounds.Right, y + 4);
        y += 18;

        // ── BILL TO ─────────────────────────────────────────────────────
        g.DrawString("BILL TO:", headFont, navyBrush, bounds.Left, y);
        y += 18;
        g.DrawString(invoice.PartyName,                    headFont,   darkBrush, bounds.Left, y); y += 17;
        g.DrawString(invoice.PartyAddress,                 normalFont, darkBrush, bounds.Left, y); y += 17;
        g.DrawString($"Mobile : {invoice.PartyMobile}",   normalFont, darkBrush, bounds.Left, y); y += 17;
        g.DrawString($"GST No : {invoice.PartyGstNumber}", normalFont, darkBrush, bounds.Left, y); y += 26;

        // ── ITEMS TABLE ─────────────────────────────────────────────────
        // bounds.Width ≈ 727  (A4 with 50-unit margins)
        // Column widths: Desc 285 | Qty 55 | Price 85 | GST% 65 | Disc 75 | Total ~162
        const int cDesc  = 4;
        const int cQty   = 289;
        const int cPrice = 344;
        const int cGst   = 429;
        const int cDisc  = 494;
        const int cTotal = 569;   // starts at bounds.Left + 569 = 619; ends at bounds.Right = 777

        const int rowH = 22;

        var hdr = new Rectangle(bounds.Left, y, bounds.Width, rowH);
        g.FillRectangle(navyBrush, hdr);
        g.DrawString("ITEM DESCRIPTION", headFont, whiteBrush, bounds.Left + cDesc,  y + 4);
        g.DrawString("QTY",              headFont, whiteBrush, bounds.Left + cQty,   y + 4);
        g.DrawString("PRICE",            headFont, whiteBrush, bounds.Left + cPrice, y + 4);
        g.DrawString("GST%",             headFont, whiteBrush, bounds.Left + cGst,   y + 4);
        g.DrawString("DISC",             headFont, whiteBrush, bounds.Left + cDisc,  y + 4);
        g.DrawString("TOTAL",            headFont, whiteBrush, bounds.Left + cTotal, y + 4);
        y += rowH + 2;

        bool alt = false;
        foreach (var item in invoice.Items)
        {
            if (alt)
            {
                using var ab = new SolidBrush(rowAlt);
                g.FillRectangle(ab, new Rectangle(bounds.Left, y, bounds.Width, rowH));
            }
            g.DrawString(item.Description,              normalFont, darkBrush, bounds.Left + cDesc,  y + 3);
            g.DrawString(item.Quantity.ToString(),       normalFont, darkBrush, bounds.Left + cQty,   y + 3);
            g.DrawString(item.UnitPrice.ToString("F2"),  normalFont, darkBrush, bounds.Left + cPrice, y + 3);
            g.DrawString(item.GstPercent.ToString("F2"), normalFont, darkBrush, bounds.Left + cGst,   y + 3);
            g.DrawString(item.Discount.ToString("F2"),   normalFont, darkBrush, bounds.Left + cDisc,  y + 3);
            g.DrawString(item.Total.ToString("F2"),      normalFont, darkBrush, bounds.Left + cTotal, y + 3);
            y += rowH;
            alt = !alt;
        }

        y += 8;
        g.DrawLine(navyPen, bounds.Left, y, bounds.Right, y);
        y += 16;

        // ── TOTALS ──────────────────────────────────────────────────────
        // Label left-edge, value right-aligned to bounds.Right - 8
        int tlabel  = bounds.Right - 240;
        int tright  = bounds.Right - 8;   // values right-align to this x

        void TotalRow(string label, string val, Font font, SolidBrush brush)
        {
            g.DrawString(label, font, brush, tlabel, y);
            var vsz = g.MeasureString(val, font);
            g.DrawString(val, font, brush, tright - vsz.Width, y);
            y += 22;
        }

        TotalRow("Sub Total :", $"Rs. {invoice.SubTotal:F2}",      normalFont, grayBrush);
        TotalRow("GST :",       $"Rs. {invoice.TotalGst:F2}",      normalFont, grayBrush);
        TotalRow("Discount :",  $"Rs. {invoice.TotalDiscount:F2}", normalFont, grayBrush);

        // Divider line drawn AFTER the full 22 px row height — never cuts through text
        y += 6;
        g.DrawLine(tealPen, tlabel, y, bounds.Right, y);
        y += 10;

        TotalRow("GRAND TOTAL :", $"Rs. {invoice.GrandTotal:F2}", totalFont, navyBrush);
        y += 20;

        // ── FOOTER ──────────────────────────────────────────────────────
        g.DrawLine(tealPen, bounds.Left, y, bounds.Right, y);
        y += 10;
        g.DrawString("Thank you for choosing SS Traders!  We value your business.",
                     smallFont, grayBrush, bounds.Left, y);
        var cg  = "Computer generated invoice.";
        var cgw = g.MeasureString(cg, smallFont);
        g.DrawString(cg, smallFont, grayBrush, bounds.Right - cgw.Width, y);
    }
}
