// debug fou here
// console.log("Content-Type text/plain");
/* express test server.js action bellow */
const express = require("express");
const dotenv = require("dotenv").config();

const app = express();

const port = process.env.PORT || 5000;

// const port = 5000;

app.use(express.json());

app.use("/api/contacts", require("./routes/Contactsroutes"));

app.listen(port, () => {
  console.log(`Server running on port ${port}`);
});

// const hostname = "127.0.0.1";
// const port = 3000;

// const server = createServer((req, res) => {
//   res.statusCode = 200;
//   res.setHeader("Content-Type", "text/plain");
//   res.end("Hello World");
// });

// server.listen(port, hostname, () => {
//   console.log(`Server running at http://${hostname}:${port}/`);
// });
