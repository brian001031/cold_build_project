import React, { useEffect, useRef } from "react";
import "./langswitch.css";

const LanguageSwitcher = ({ lang, handleLanguageChange, languages }) => {
  const [isLangMenuOpen, setIsLangMenuOpen] = React.useState(false);
  const menuRef = useRef(null);

  // 點擊外部時關閉選單
  useEffect(() => {
    const handleClickOutside = (event) => {
      if (menuRef.current && !menuRef.current.contains(event.target)) {
        setIsLangMenuOpen(false);
      }
    };

    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  return (
    <div
      className="d-flex"
      style={{ marginLeft: "1.2rem", height: "auto", padding: " 6px 12px" }}
    >
      <nav className="language-switcher">
        <button
          className="language-toggle"
          onClick={() => setIsLangMenuOpen(!isLangMenuOpen)}
          aria-haspopup="true"
          aria-expanded={isLangMenuOpen}
        >
          {languages.find((language) => language.code === lang)?.label ||
            "選擇語系"}
        </button>
        {isLangMenuOpen && (
          <ul className="language-menu" role="menu">
            {languages.map((language) => (
              <li key={language.code}>
                <button
                  className="language-item"
                  onClick={() => handleLanguageChange(language.code)}
                  role="menuitem"
                >
                  {language.label}
                </button>
              </li>
            ))}
          </ul>
        )}
      </nav>
    </div>
  );
};

export default LanguageSwitcher;
