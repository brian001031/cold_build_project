/**
 * 工具函數：批量替換檔案中的 toast 調用
 * 這個函數可以用來快速轉換 toast.error 和 toast.success 為新的 onShowMessage 模式
 */

// 需要修改的 toast 調用模式
const toastPatterns = [
    // toast.error 模式
    {
        pattern: /toast\.error\("([^"]+)"\);/g,
        replacement: (match, message) => {
            return `const errorMessage = "${message}";
            setErrorText(errorMessage);
            if (onShowMessage) onShowMessage('error', errorMessage);`;
        }
    },
    
    // toast.success 模式
    {
        pattern: /toast\.success\("([^"]+)"\);/g,
        replacement: (match, message) => {
            return `const successMessage = "${message}";
            if (onShowMessage) onShowMessage('success', successMessage);`;
        }
    }
];

// 需要在組件中添加的參數
const componentSignatureUpdates = [
    {
        // 為組件添加 onShowMessage 參數
        pattern: /const (\w+) = \(\{([^}]+)\}\) =>/g,
        replacement: (match, componentName, params) => {
            if (!params.includes('onShowMessage')) {
                return `const ${componentName} = ({${params}, onShowMessage}) =>`;
            }
            return match;
        }
    }
];

// 需要添加的 state
const stateToAdd = `const [errorText, setErrorText] = useState("");`;

// 使用說明:
// 1. 在組件參數中添加 onShowMessage
// 2. 添加 errorText state
// 3. 替換所有 toast.error 和 toast.success 調用
// 4. 在主頁面中傳遞 onShowMessage 參數

export { toastPatterns, componentSignatureUpdates, stateToAdd };
