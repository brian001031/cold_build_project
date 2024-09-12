//@desc Get all contacts
//@desc GET /api/contacts
//@access public

const getContacts = (req, res) => {
  res.status(200).json({ message: "Get ALL contacts!" });
};

//@desc Create all contacts
//@desc POST /api/contacts
//@access public

const createContacts = (req, res) => {
  console.log("recive data = ", req.body);
  const { name, IPv4, macaddress } = req.body;
  if (!name || !IPv4 || !macaddress) {
    res.status(401);
    throw new Error("All fields are mandatory !");
  }
  res.status(201).json({ message: " Create New contacts!" });
};

//@desc Get select id contacts
//@desc GET /api/contacts/:id
//@access public

const getContact = (req, res) => {
  res.status(200).json({ message: `Get Contacts for ${req.params.id}` });
};

//@desc update select id contacts
//@desc UPDATE /api/contacts/:id
//@access public

const updateContact = (req, res) => {
  res.status(200).json({ message: `Update Contacts for ${req.params.id}` });
};

//@desc delete select id contacts
//@desc DELETE /api/contacts/:id
//@access public

const deleteContact = (req, res) => {
  res.status(200).json({ message: `Delect Contacts for ${req.params.id}` });
};

module.exports = {
  getContacts,
  createContacts,
  getContact,
  updateContact,
  deleteContact,
};
