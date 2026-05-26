import React, { useState } from "react";
import {
  FaTh,
  FaBars,
  FaUserAlt,
  FaRegChartBar,
  FaRegClock,
  FaCommentAlt,
  FaShoppingBag,
  FaThList,
  FaAirbnb,
} from "react-icons/fa";
import { NavLink } from "react-router-dom";
// import Analytics from "../pages/ClassRecycleChart/Sidebar/index_analytics";
// import About from "../pages/ClassRecycleChart/Sidebar/index_about";

const Sidebar = ({ children }) => {
  const [isOpen, setIsOpen] = useState(false);
  const toggle = () => setIsOpen(!isOpen);

  const menuItem = [
    // {
    //   path: "/",
    //   name: "Dashboard",
    //   icon: <FaTh />,
    // },
    {
      path: "/sidebar/analyticsyear",
      name: "年統計數據",
      icon: <FaRegChartBar />,
      // Element: <Analytics />,
    },
    {
      path: "/sidebar/dynamicday",
      name: "每日動態處理",
      icon: <FaRegClock />,
      // Element: <About />,
    },
    {
      path: "/sidebar/coating_presure_diagram",
      name: "塗佈無塵室內靜壓監測",
      icon: <FaAirbnb />,
    },
  ];

  return (
    <div className="container">
      <div style={{ width: isOpen ? "200px" : "50px" }} className="sidebar">
        <div className="top_section">
          <h1
            style={{
              display: isOpen ? "block" : "none",
            }}
            className="logo"
          >
            回收數據
          </h1>
          <div style={{ marginLeft: isOpen ? "50px" : "0px" }} className="bars">
            <FaBars onClick={toggle} />
          </div>
        </div>
        {menuItem.map((item, index) => (
          <NavLink
            to={item.path}
            key={index}
            className="link"
            activeclassName="active"
          >
            <div className="icon">{item.icon}</div>
            <div
              style={{ display: isOpen ? "block" : "none" }}
              className="link_text"
            >
              {item.name}
            </div>
          </NavLink>
        ))}
      </div>
      <main>{children}</main>
    </div>
  );
};

export default Sidebar;
