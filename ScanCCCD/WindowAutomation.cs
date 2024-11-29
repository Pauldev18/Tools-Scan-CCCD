using Interop.UIAutomationClient;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ScanCCCD
{
    public class WindowAutomation
    {
        private static readonly IUIAutomation Automation = new CUIAutomation();

        public static IUIAutomationElement CachedWindow = null;

        public static IUIAutomationElement FindWindowByAutomationId(string automationId)
        {
            if (CachedWindow != null)
            {
                return CachedWindow;
            }

            // Nếu chưa có, tiến hành tìm kiếm
            IUIAutomationElement rootElement = Automation.GetRootElement();

            // Sử dụng ControlType.Window để tìm cửa sổ và tìm kiếm theo AutomationId
            var condition = Automation.CreatePropertyCondition(UIA_PropertyIds.UIA_ControlTypePropertyId, UIA_ControlTypeIds.UIA_WindowControlTypeId);
            IUIAutomationElementArray windows = rootElement.FindAll(Interop.UIAutomationClient.TreeScope.TreeScope_Descendants, condition);

            for (int i = 0; i < windows.Length; i++)
            {
                IUIAutomationElement window = windows.GetElement(i);

                // So sánh AutomationId của cửa sổ với giá trị truyền vào
                string currentAutomationId = window.CurrentAutomationId;
                if (currentAutomationId == automationId)
                {
                    CachedWindow = window; // Lưu vào biến toàn cục
                    return CachedWindow;
                }
            }

            return null; // Không tìm thấy cửa sổ
        }



        // Biến toàn cục để lưu phần tử đã tìm thấy
        private static Dictionary<string, IUIAutomationElement> CachedElements = new Dictionary<string, IUIAutomationElement>();

        public static void TypeTextAndPerformAction(IUIAutomationElement windowElement, string elementAutomationId, string textToEnter)
        {
            try
            {
                // Kiểm tra xem phần tử đã được lưu trong bộ nhớ cache chưa
                if (!CachedElements.TryGetValue(elementAutomationId, out IUIAutomationElement targetElement))
                {
                    // Nếu chưa có, tìm phần tử và lưu lại
                    var condition = Automation.CreatePropertyCondition(UIA_PropertyIds.UIA_AutomationIdPropertyId, elementAutomationId);
                    targetElement = windowElement.FindFirst(Interop.UIAutomationClient.TreeScope.TreeScope_Descendants, condition);

                    if (targetElement != null)
                    {
                        CachedElements[elementAutomationId] = targetElement; // Lưu vào cache
                    }
                    else
                    {
                        Console.WriteLine($"Không tìm thấy phần tử với AutomationId: {elementAutomationId}");
                        return;
                    }
                }

                // Thao tác với phần tử đã được tìm hoặc lưu từ trước
                targetElement.SetFocus();

                // Xóa nội dung hiện tại (nếu phần tử là TextBox)
                SendKeys.SendWait("^(A)"); // Chọn tất cả
                SendKeys.SendWait("{DEL}"); // Xóa

                // Nhập văn bản
                SendKeys.SendWait(textToEnter);
                SendKeys.SendWait("{TAB}"); // Chuyển sang phần tử tiếp theo

              
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi thao tác với phần tử: {ex.Message}");
            }
        }

/*        // Biến toàn cục để lưu trữ các phần tử đã tìm thấy
        private static Dictionary<string, IUIAutomationElement> CachedElements2 = new Dictionary<string, IUIAutomationElement>();

        public static void ClearTextBoxByAutomationId(IUIAutomationElement windowElement, string textBoxAutomationId)
        {
            try
            {
                // Kiểm tra xem phần tử đã có trong cache chưa
                if (!CachedElements2.TryGetValue(textBoxAutomationId, out IUIAutomationElement textBoxElement))
                {
                    // Tìm TextBox nếu chưa có trong cache
                    var condition = Automation.CreatePropertyCondition(UIA_PropertyIds.UIA_AutomationIdPropertyId, textBoxAutomationId);
                    textBoxElement = windowElement.FindFirst(Interop.UIAutomationClient.TreeScope.TreeScope_Descendants, condition);

                    // Nếu tìm thấy, lưu lại vào cache
                    if (textBoxElement != null)
                    {
                        CachedElements2[textBoxAutomationId] = textBoxElement;
                    }
                    else
                    {
                        Console.WriteLine($"Không tìm thấy TextBox với AutomationId: {textBoxAutomationId}");
                        return;
                    }
                }

                // Sử dụng ValuePattern để thao tác với TextBox
                var valuePattern = textBoxElement.GetCurrentPattern(UIA_PatternIds.UIA_ValuePatternId) as IUIAutomationValuePattern;

                if (valuePattern != null)
                {
                    // Đặt giá trị TextBox thành rỗng
                    valuePattern.SetValue(string.Empty);

                    // Gửi phím TAB để chuyển qua phần tử khác
                    SendKeys.SendWait("{TAB}");
                }
                else
                {
                    Console.WriteLine("Không thể thao tác với TextBox này thông qua ValuePattern.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi thao tác với TextBox: {ex.Message}");
            }
        }*/

        // Biến toàn cục để lưu trữ phần tử đã tìm thấy (cache)
        private static Dictionary<string, IUIAutomationElement> TextBoxCache = new Dictionary<string, IUIAutomationElement>();

        public static void TypeTextOnly(IUIAutomationElement windowElement, string textBoxAutomationId, string textToEnter)
        {
            try
            {
                // Kiểm tra xem phần tử đã có trong cache chưa
                if (!TextBoxCache.TryGetValue(textBoxAutomationId, out IUIAutomationElement textBoxElement))
                {
                    // Tìm TextBox nếu chưa có trong cache
                    var condition = Automation.CreatePropertyCondition(UIA_PropertyIds.UIA_AutomationIdPropertyId, textBoxAutomationId);
                    textBoxElement = windowElement.FindFirst(Interop.UIAutomationClient.TreeScope.TreeScope_Descendants, condition);

                    // Nếu tìm thấy, lưu lại vào cache
                    if (textBoxElement != null)
                    {
                        TextBoxCache[textBoxAutomationId] = textBoxElement;
                    }
                    else
                    {
                        Console.WriteLine($"Không tìm thấy TextBox với AutomationId: {textBoxAutomationId}");
                        return;
                    }
                }

                // Đảm bảo TextBox có Focus
                textBoxElement.SetFocus();

                // Xóa nội dung hiện tại
                var valuePattern = textBoxElement.GetCurrentPattern(UIA_PatternIds.UIA_ValuePatternId) as IUIAutomationValuePattern;

                if (valuePattern != null)
                {
                    // Nhập văn bản mới vào TextBox
                    valuePattern.SetValue(textToEnter);

                    // Gửi phím ENTER
                    SendKeys.SendWait("{ENTER}");

                    // Gửi phím TAB để chuyển qua phần tử tiếp theo
                    SendKeys.SendWait("{TAB}");
                }
                else
                {
                    Console.WriteLine("Không thể thao tác với TextBox này thông qua ValuePattern.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi thao tác với TextBox: {ex.Message}");
            }
        }

        // Biến toàn cục để lưu cache các TextBox theo AutomationId và Instance
        private static Dictionary<(string AutomationId, int Instance), IUIAutomationElement> TextBoxInstanceCache = new Dictionary<(string, int), IUIAutomationElement>();

        public static void TypeTextDate(IUIAutomationElement windowElement, string textBoxAutomationId, int instance, string textToEnter)
        {
            try
            {
                // Tạo khóa cache dựa trên AutomationId và Instance
                var cacheKey = (AutomationId: textBoxAutomationId, Instance: instance);

                // Kiểm tra xem phần tử đã có trong cache chưa
                if (!TextBoxInstanceCache.TryGetValue(cacheKey, out IUIAutomationElement textBoxElement))
                {
                    // Tạo điều kiện tìm kiếm các TextBox có cùng AutomationId
                    IUIAutomationCondition condition = Automation.CreatePropertyCondition(UIA_PropertyIds.UIA_AutomationIdPropertyId, textBoxAutomationId);

                    // Tìm tất cả các phần tử thỏa mãn điều kiện
                    IUIAutomationElementArray elements = windowElement.FindAll(Interop.UIAutomationClient.TreeScope.TreeScope_Descendants, condition);

                    // Kiểm tra nếu số lượng phần tử đủ để lấy theo instance
                    if (elements != null && elements.Length > instance)
                    {
                        // Lấy TextBox dựa trên instance index
                        textBoxElement = elements.GetElement(instance);

                        // Lưu vào cache
                        TextBoxInstanceCache[cacheKey] = textBoxElement;
                    }
                    else
                    {
                        Console.WriteLine($"Không tìm thấy TextBox với AutomationId: {textBoxAutomationId} và Instance: {instance}");
                        return;
                    }
                }

                // Kiểm tra nếu control hỗ trợ ValuePattern
                var valuePattern = textBoxElement.GetCurrentPattern(UIA_PatternIds.UIA_ValuePatternId) as IUIAutomationValuePattern;

                if (valuePattern != null)
                {
                    textBoxElement.SetFocus();
                    SendKeys.SendWait(textToEnter); 

                    // Chuyển đến phần tử tiếp theo (nếu cần)
                    SendKeys.SendWait("{TAB}");

                    Console.WriteLine($"Đã đặt giá trị: {textToEnter} vào TextBox với AutomationId: {textBoxAutomationId} và Instance: {instance}");
                }
                else
                {
                    Console.WriteLine("Control không hỗ trợ ValuePattern, thử phương pháp khác.");

                    // Sử dụng cách khác nếu không có ValuePattern
                    textBoxElement.SetFocus();
                    SendKeys.SendWait(textToEnter);
                    SendKeys.SendWait("{TAB}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi thao tác với TextBox: {ex.Message}");
            }
        }


        // Tạo cache để lưu trữ các phần tử đã tìm kiếm
        private static readonly Dictionary<string, IUIAutomationElement> ComboBoxCache = new Dictionary<string, IUIAutomationElement>();

        public static void SelectComboBoxOptionWithCache(IUIAutomationElement windowElement, string comboBoxAutomationId, int index)
        {
            try
            {
                if (windowElement == null)
                {
                    Console.WriteLine("`windowElement` is null. Please ensure the window element is correctly retrieved.");
                    return;
                }

                if (string.IsNullOrEmpty(comboBoxAutomationId))
                {
                    Console.WriteLine("`comboBoxAutomationId` is null or empty. Provide a valid AutomationId.");
                    return;
                }

                IUIAutomationElement comboBoxElement;

                // Kiểm tra cache trước
                if (ComboBoxCache.ContainsKey(comboBoxAutomationId))
                {
                    comboBoxElement = ComboBoxCache[comboBoxAutomationId];
                    Console.WriteLine($"ComboBox đã được tìm thấy trong cache với AutomationId: {comboBoxAutomationId}");
                }
                else
                {
                    // Tìm ComboBox theo AutomationId
                    IUIAutomationCondition condition = Automation.CreatePropertyCondition(UIA_PropertyIds.UIA_AutomationIdPropertyId, comboBoxAutomationId);
                    comboBoxElement = windowElement.FindFirst(Interop.UIAutomationClient.TreeScope.TreeScope_Descendants, condition);

                    if (comboBoxElement == null)
                    {
                        Console.WriteLine($"Không tìm thấy ComboBox với AutomationId: {comboBoxAutomationId}");
                        return;
                    }

                    // Lưu vào cache
                    ComboBoxCache[comboBoxAutomationId] = comboBoxElement;
                    Console.WriteLine($"Đã lưu ComboBox vào cache với AutomationId: {comboBoxAutomationId}");
                }

                // Kiểm tra xem phần tử có hỗ trợ ValuePattern không
                var comboBoxPattern = comboBoxElement.GetCurrentPattern(UIA_PatternIds.UIA_ValuePatternId) as IUIAutomationValuePattern;

                if (comboBoxPattern == null)
                {
                    Console.WriteLine("ComboBox không hỗ trợ ValuePattern.");
                    return;
                }

                // Tạo danh sách các tùy chọn (có thể mở rộng)
                var options = new List<string>
                {
                    "In 1 GPLX Không thời hạn",
                    "In 1 GPLX Có thời hạn",
                    "Nam",
                    "Nữ"
                };

                // Kiểm tra nếu index hợp lệ
                if (index < 0 || index >= options.Count)
                {
                    Console.WriteLine($"Index không hợp lệ. Chỉ số phải nằm trong khoảng từ 0 đến {options.Count - 1}.");
                    return;
                }

                // Đặt giá trị vào ComboBox
                comboBoxPattern.SetValue(options[index]);
                Console.WriteLine($"Đã đặt giá trị: {options[index]} vào ComboBox với AutomationId: {comboBoxAutomationId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi thao tác với ComboBox: {ex.Message}");
            }
        }


        // Cache lưu trữ các phần tử đã tìm kiếm
        private static Dictionary<string, IUIAutomationElement> cachedTextBoxElements = new Dictionary<string, IUIAutomationElement>();
        private static Dictionary<string, IUIAutomationElement> cachedSuggestionListElements = new Dictionary<string, IUIAutomationElement>();

        public static void TypeTextAndSelectFirstSuggestion(IUIAutomationElement windowElement, string textBoxAutomationId, string[] addressParts)
        {
            try
            {
                if (windowElement == null)
                {
                    Console.WriteLine("`windowElement` is null. Ensure the window element is correctly retrieved.");
                    return;
                }

                if (string.IsNullOrEmpty(textBoxAutomationId))
                {
                    Console.WriteLine("`textBoxAutomationId` is null or empty. Provide a valid AutomationId.");
                    return;
                }

                // Kiểm tra trong cache
                if (!cachedTextBoxElements.ContainsKey(textBoxAutomationId))
                {
                    // Tạo điều kiện tìm kiếm TextBox theo AutomationId
                    IUIAutomationCondition condition = Automation.CreatePropertyCondition(UIA_PropertyIds.UIA_AutomationIdPropertyId, textBoxAutomationId);

                    // Tìm TextBox dựa trên điều kiện
                    IUIAutomationElement textBoxElement = windowElement.FindFirst(Interop.UIAutomationClient.TreeScope.TreeScope_Descendants, condition);

                    if (textBoxElement == null)
                    {
                        Console.WriteLine($"Không tìm thấy TextBox với AutomationId: {textBoxAutomationId}");
                        return;
                    }

                    // Lưu phần tử vào cache
                    cachedTextBoxElements[textBoxAutomationId] = textBoxElement;
                }

                IUIAutomationElement cachedTextBox = cachedTextBoxElements[textBoxAutomationId];

                // Đảm bảo TextBox có Focus
                cachedTextBox.SetFocus();

               /* // Chọn toàn bộ nội dung (Ctrl + A) và xóa nội dung cũ (Del)
                SendKeys.SendWait("^(A)");
                SendKeys.SendWait("{DEL}");*/

                // Gõ từng phần của địa chỉ vào TextBox
                foreach (string part in addressParts)
                {
                    SendKeys.SendWait("^{HOME}");
                    SendKeys.SendWait(part + " ");
                }

                // Gửi phím Enter để xác nhận
                SendKeys.SendWait("{ENTER}");

                // Kiểm tra trong cache đối với danh sách gợi ý
                if (!cachedSuggestionListElements.ContainsKey(textBoxAutomationId))
                {
                    // Tìm danh sách gợi ý (nếu có)
                    IUIAutomationCondition suggestionCondition = Automation.CreatePropertyCondition(UIA_PropertyIds.UIA_ControlTypePropertyId, UIA_ControlTypeIds.UIA_ListControlTypeId);
                    IUIAutomationElement suggestionList = cachedTextBox.FindFirst(Interop.UIAutomationClient.TreeScope.TreeScope_Descendants, suggestionCondition);

                    if (suggestionList == null)
                    {
                        Console.WriteLine("Không tìm thấy danh sách gợi ý.");
                        return;
                    }

                    // Lưu danh sách gợi ý vào cache
                    cachedSuggestionListElements[textBoxAutomationId] = suggestionList;
                }

                IUIAutomationElement cachedSuggestionList = cachedSuggestionListElements[textBoxAutomationId];

                // Lấy danh sách các phần tử trong gợi ý
                IUIAutomationElementArray suggestionItems = cachedSuggestionList.FindAll(Interop.UIAutomationClient.TreeScope.TreeScope_Children, null);

                if (suggestionItems != null && suggestionItems.Length > 0)
                {
                    // Chọn phần tử đầu tiên trong danh sách gợi ý
                    IUIAutomationElement firstSuggestion = suggestionItems.GetElement(0);
                    firstSuggestion.SetFocus();
                    SendKeys.SendWait("{ENTER}");
                    Console.WriteLine("Gợi ý đầu tiên đã được chọn.");
                }
                else
                {
                    Console.WriteLine("Không tìm thấy gợi ý nào.");
                }

                // Gửi phím Tab để di chuyển đến phần tử tiếp theo
                SendKeys.SendWait("{TAB}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi thao tác với TextBox: {ex.Message}");
            }
        }

        // Lưu cache cho các phần tử tìm thấy
        private static Dictionary<string, IUIAutomationElement> elementCache = new Dictionary<string, IUIAutomationElement>();

        public static void ClickButtonByAutomationId(IUIAutomationElement windowElement, string buttonAutomationId)
        {
            try
            {
                if (windowElement == null)
                {
                    Console.WriteLine("windowElement is null. Ensure the window element is correctly retrieved.");
                    return;
                }

                if (string.IsNullOrEmpty(buttonAutomationId))
                {
                    Console.WriteLine("buttonAutomationId is null or empty. Ensure a valid AutomationId is provided.");
                    return;
                }

                // Kiểm tra xem button đã có trong cache chưa
                if (elementCache.ContainsKey(buttonAutomationId))
                {
                    // Lấy phần tử từ cache
                    var buttonElement = elementCache[buttonAutomationId];
                    PerformClick(buttonElement, buttonAutomationId);
                }
                else
                {
                    // Tạo điều kiện tìm kiếm Button theo AutomationId
                    IUIAutomationCondition condition = Automation.CreatePropertyCondition(UIA_PropertyIds.UIA_AutomationIdPropertyId, buttonAutomationId);

                    // Tìm Button dựa trên điều kiện
                    IUIAutomationElement buttonElement = windowElement.FindFirst(Interop.UIAutomationClient.TreeScope.TreeScope_Descendants, condition);

                    if (buttonElement != null)
                    {
                        // Lưu phần tử vào cache
                        elementCache[buttonAutomationId] = buttonElement;
                        PerformClick(buttonElement, buttonAutomationId);
                    }
                    else
                    {
                        Console.WriteLine($"Không tìm thấy Button với AutomationId: {buttonAutomationId}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi thao tác với Button: {ex.Message}");
            }
        }

        private static void PerformClick(IUIAutomationElement buttonElement, string buttonAutomationId)
        {
            try
            {
                // Kiểm tra xem phần tử có hỗ trợ InvokePattern (hỗ trợ click)
                IUIAutomationInvokePattern invokePattern = buttonElement.GetCurrentPattern(UIA_PatternIds.UIA_InvokePatternId) as IUIAutomationInvokePattern;

                if (invokePattern != null)
                {
                    // Nhấn nút
                    invokePattern.Invoke();
                    Console.WriteLine($"Đã click vào Button với AutomationId: {buttonAutomationId}");
                }
                else
                {
                    Console.WriteLine($"Button không hỗ trợ InvokePattern, không thể click.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi thao tác với Button: {ex.Message}");
            }
        }

    }
}
