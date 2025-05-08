import { Link } from "react-router-dom";
import { shulin_mainbox } from "../../data";
import "./shulin_info.scss";
function shulin_info() {
  return (
    <div className="shulin_info">
      {shulin_mainbox.map((item) => (
        <div className={`box box${item.id}`}>
          <span className="title">{item.title}</span>
          <div className="image-container">
            <a href={item.url}>
              <img className="image" src={item.icon} alt=""></img>
            </a>
          </div>
        </div>
      ))}
      ;
    </div>
  );
}

export default shulin_info;
