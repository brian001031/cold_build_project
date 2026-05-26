// LanguageMultilingual.js
import React, {
  createContext,
  useContext,
  useState,
  useEffect,
  startTransition,
} from "react";
import { IntlProvider } from "react-intl"; // 引入 IntlProvider

export const LanguageMulti = createContext();

export const languages = [
  { code: "zh-Hant", label: "中文(繁)" },
  { code: "en", label: "English" },
  // { code: "jp", label: "日本語" },
];

const defaultLang = "zh-Hant"; // 預設語系


// 新增的扁平化函式：將巢狀物件轉換為扁平物件
const flattenMessages = (nestedMessages, prefix = '') => {
  return Object.keys(nestedMessages).reduce((messages, key) => {
    const value = nestedMessages[key];
    const prefixedKey = prefix ? `${prefix}.${key}` : key;

    if (typeof value === 'object' && value !== null && !Array.isArray(value)) {
      Object.assign(messages, flattenMessages(value, prefixedKey));
    } else {
      messages[prefixedKey] = value;
    }

    return messages;
  }, {});
};

export const LanguageProvider = ({ children }) => {
  // const [lang, setLang] = useState("zh-Hant"); // 預設語系
  const [lang, setLang] = useState(() => {
    return localStorage.getItem("lang") || defaultLang;
  });
  const [locale, setLocale] = useState({});

  useEffect(() => {
    const fetchLangFile = async () => {
      try {
        // const resp = await fetch(`/lang/${lang}.json`);
        const resp = await fetch(`${process.env.PUBLIC_URL}/lang/${lang}.json`);
        if (!resp.ok) throw new Error(`Failed to fetch ${lang}.json`);
        const data = await resp.json();
        // setLocale(data);  // <-- 當 setState 時造成 re-render，而此 render 涉及 Suspense
        // Use startTransition to make this update non-blocking

         const flatMessages = flattenMessages(data); 

        startTransition(() => {
          setLocale(flatMessages);
        });
        // console.log(`已載入${lang}.json語系`);
      } catch (error) {
        console.error("Error loading language file:", error);
        // Fallback 回預設語系
        if (lang !== defaultLang) {
          setLang(defaultLang);
          localStorage.setItem("lang", defaultLang);
        } else {
          setLocale({});
        }
      }
    };

    fetchLangFile();
  }, [lang]);

  const changeLang = (newLang) => {
    startTransition(() => {
      setLang(newLang);
      localStorage.setItem("lang", newLang);
    });
  };

  console.log("目前載入語系:", lang, locale);

  return (
    <LanguageMulti.Provider value={{ lang, setLang: changeLang, locale }}>
      <IntlProvider
        key={lang}
        locale={lang}
        messages={locale || {}} // fallback 空物件，避免爆錯
        defaultLocale={"zh-Hant"} // 如果找不到對應語言的 key，預設繁體中文
      >
        {children}
      </IntlProvider>
    </LanguageMulti.Provider>
  );

  return (
    <LanguageMulti.Provider value={{ lang, setLang: changeLang, locale }}>
      {!locale || Object.keys(locale).length === 0 ? (
        <div>載入語系|Loading language...</div>
      ) : (
        <IntlProvider
          key={lang}
          locale={lang}
          messages={locale}
          defaultLocale="zh-Hant"
        >
          {children}
        </IntlProvider>
      )}
    </LanguageMulti.Provider>
  );
};

export const useLanguage = () => {
  const context = useContext(LanguageMulti);
  if (!context)
    throw new Error("useLanguage must be used inside LanguageProvider");
  return context;
};
