import React, { useState } from "react";
import { Card, Form, Button, Image } from "react-bootstrap";
import Table from "react-bootstrap/Table";
import "../pages/ClassRecycleChart/Sidebar/index.scss";

const DynamicTable = ({ data }) => {
  const columns = data.length > 0 ? Object.keys(data[0]) : [];

  return (
    <div className="recyclechart_dynamic">
      <p
        style={{
          textAlign: "center",
          fontSize: "300%",
          fontStyle: "bold",
          color: "#EA0000",
        }}
      >
        單位(公斤/日)
      </p>
      {columns && (
        <Form>
          <Table
            style={{ textAlign: "center", verticalAlign: "middle" }}
            striped
            bordered
            hover
          >
            <thead>
              {/* <tr>
                {columns.map((column, index) => (
                  <th key={index}>{"序號:" + parseInt(index + 1)}</th>
                ))}
              </tr> */}
              <tr>
                {columns.map((column, index) => (
                  // <th key={index}>{column}</th>
                  <th>{column}</th>
                ))}
              </tr>
            </thead>
            <tbody>
              {data.map((row, rowIndex) => (
                <tr key={rowIndex}>
                  {columns.map((column, colIndex) => (
                    <td key={colIndex}>{row[column]}</td>
                  ))}
                </tr>
              ))}
            </tbody>
          </Table>
        </Form>
      )}
    </div>
  );
};

export default DynamicTable;
