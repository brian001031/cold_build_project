const express = require("express");
const router = express.Router();

// bellow direct api function
const {
  getContacts,
  createContacts,
  getContact,
  updateContact,
  deleteContact,
} = require("../controllers/contactscontrollers");

//各指定route CURD action
router.route("/").get(getContacts).post(createContacts);

router.route("/:id").get(getContact).put(updateContact).delete(deleteContact);

// router.route("/").get((req, res) => {
//   res.status(200).json({ message: " Get All Contacts!" });
// });

// router.route("/").post((req, res) => {
//   res.status(200).json({ message: "Create New Contacts!" });
// });

// router.route("/:id").get((req, res) => {
//   res.status(200).json({ message: `Get Contacts for ${req.params.id}` });
// });

// router.route("/:id").put((req, res) => {
//   res.status(200).json({ message: `Update Contacts for ${req.params.id}` });
// });

// router.route("/:id").delete((req, res) => {
//   res.status(200).json({ message: `Delect Contacts for ${req.params.id}` });
// });

module.exports = router;
