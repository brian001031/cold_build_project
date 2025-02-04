import React, { useState, useEffect } from "react";
// import { useParams } from "react-router-dom";
import { BrowserRouter, Route, Routes } from "react-router-dom";
import Sidebar from "../../components/Sidebar";
// import Analytics from "./Sidebar/index_analytics";
// import About from "./Sidebar/index_about";
import axios from "axios";
import config from "../../config";
import { Link } from "react-router-dom";
import "./index.scss";
// import { Card, Form, Button, Image } from "react-bootstrap";
// import dayjs from "dayjs";
//成功提示套件
import { toast } from "react-toastify";

const RecycleChart = () => {
  // const { id } = useParams();
  const [formData, setFormData] = useState(null);

  //姓名的下拉選項
  const [options, setOptions] = useState([]);

  const handleChange = async (e) => {
    const { name, value } = e.target;
  };

  return (
    <div className="recyclechart">
      <Sidebar>
        {/* { <Route path="/" element={<Analytics />} /> } */}
        {/* <Route path="/analytics" element={<Analytics />} />
          <Route path="/about" element={<About />} /> */}

        {/* <Link to={`/recycleedit/${item.id}`}>{item.id}</Link> */}
      </Sidebar>
    </div>
  );
};

export default RecycleChart;
