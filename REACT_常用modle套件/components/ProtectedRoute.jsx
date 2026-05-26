import React from "react";
import { Navigate } from "react-router-dom";
import { useAuth } from "../context/GlobalProvider";
import { FormattedMessage } from "react-intl";

const ProtectedRoute = ({ element }) => {
  const { isAuthenticated, isLoading, user } = useAuth();

  if (user === null)
    return (
      <div style={{ fontSize: "2.1rem", fontStyle: "inherit bold" }}>
        <FormattedMessage
          id="Navigation.nolevel_view"
          defaultMessage="無權限訪問此頁面...請先登入"
        />
      </div>
    );

  return isAuthenticated || user ? element : <Navigate to="/" replace />;
};

export default ProtectedRoute;
