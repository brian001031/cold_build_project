import React from "react";
import { Carousel } from "react-bootstrap";
import config from "../../config";
import { FaChevronLeft, FaChevronRight } from "react-icons/fa";

const FactoryPhotoCarousel = ({ photoPaths }) => {
  return (
    <Carousel
      style={{ width: "300px" }}
      prevIcon={<FaChevronLeft style={{ color: "black" }} />} // 設置左箭頭的顏色為黑色
      nextIcon={<FaChevronRight style={{ color: "black" }} />} // 設置右箭頭的顏色為黑色
    >
      {photoPaths.map((photo, index) => (
        <Carousel.Item key={index}>
          <img
            className="d-block w-100"
            src={`${config.apiBaseUrl}/factoryuploads/${photo}`}
            style={{ height: "200px", objectFit: "contain" }}
            alt={`Slide ${index + 1}`}
          />
        </Carousel.Item>
      ))}
    </Carousel>
  );
};

export default FactoryPhotoCarousel;
