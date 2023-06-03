import Bugsnag from "@bugsnag/js";
import loadable from "@loadable/component";
import React, { useEffect, useState } from "react";
import { shallowEqual, useDispatch, useSelector } from "react-redux";
import { BrowserRouter as Router, Navigate, Route } from "react-router-dom";

import Avatar from "./components/Avatar";
import Loader from "./components/Loader";
import { fetchCurrentUser } from "./modules/currentUser/currentUser";
import { RootState } from "./rootReducer";

const AsyncPage = loadable((props) => import(`./pages/${props.page}`), {
  fallback: <Loader />,
});

const PUBLIC_ROUTES = ["/privacy", /\/referees\/\d$/];

const App = () => {
  const [redirectTo, setRedirectTo] = useState<string>();
  const dispatch = useDispatch();
  const { currentUser, roles, id, error } = useSelector(
    (state: RootState) => state.currentUser,
    shallowEqual
  );

  const getRedirect = () => {
    if (roles.includes("iqa_admin")) return "/admin";
    if (roles.includes("ngb_admin")) return `/national_governing_bodies/${currentUser?.ownedNgbId}`;
    if (roles.includes("referee")) return `/referees/${id}`;

    return null;
  };

  useEffect(() => {
    if (!currentUser) {
      dispatch(fetchCurrentUser());
    }
  }, [currentUser]);

  useEffect(() => {
    const isPublic = PUBLIC_ROUTES.some((route) => window.location.pathname.match(route));

    if (error && !isPublic) {
      window.location.href = `${window.location.origin}/sign_in`;
    }
  }, [error]);

  useEffect(() => {
    setRedirectTo(getRedirect());
  }, [currentUser, roles]);

  if (currentUser) Bugsnag.setUser(id);

  return (
    <Router>
      <div>
        <div className="bg-navy-blue text-right text-white py-3 px-10 flex items-center justify-end">
          <p className="flex-shrink mx-8">Management Hub</p>
          <Avatar
            firstName={currentUser?.firstName}
            lastName={currentUser?.lastName}
            roles={roles}
            userId={id}
            ownedNgbId={currentUser?.ownedNgbId}
            enabledFeatures={currentUser?.enabledFeatures}
          />
        </div>
        <Route  path="/">
          {redirectTo && <Navigate to={redirectTo} replace />}
        </Route>
        <Route
          path="/privacy"
          element={<AsyncPage page="PrivacyPolicy" />}
        />
        <Route
          path="/referees/:id"
          element={<AsyncPage page="RefereeProfile" />}
        />
        <Route
          path="/admin"
          element={<AsyncPage page="Admin" />}
        />
        <Route
          path="/admin/tests/:id"
          element={<AsyncPage page="Test" />}
        />
        <Route
          path="/referees/:refereeId/tests/:testId"
          element={<AsyncPage page="StartTest" />}
        />
        <Route
          path="/national_governing_bodies/:id"
          element={<AsyncPage page="NgbProfile" />}
        />
        <Route
          path="/import/:scope"
          element={<AsyncPage page="ImportWizard" />}
        />
        <Route
          path="/referees/:refereeId/tests"
          element={<AsyncPage page="RefereeTests" />}
        />
        {currentUser?.enabledFeatures.includes("i18n") ? (
          <Route
            path="/settings"
            element={<AsyncPage page="Settings" />}
          />
        ) : null}
      </div>
    </Router>
  );
};

export default App;
