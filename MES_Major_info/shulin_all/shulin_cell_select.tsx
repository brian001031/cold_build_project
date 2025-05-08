import { Link } from "react-router-dom";
import { shulin_cell_select_box } from "../../data";
import "./shulin_cell_select.scss";
function shulin_cell_select() {
  return (
    <div className="shulin_cell_select">
      {shulin_cell_select_box.map((item) => (
        <div className={`box box${item.id}`}>
          <span className="title">{item.title}</span>
          <div className="image-container">
            <a href={item.url}>
              <img className="image" src={item.icon} alt=" "></img>
            </a>
          </div>
          {/* <span>{item.factory}</span> */}
        </div>
      ))}
      ;<span className="span_shulintext">樹 林 廠</span>
    </div>
  );
}

export default shulin_cell_select;
