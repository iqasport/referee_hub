import Bugsnag from "@bugsnag/js";
import React, { useEffect, useState, lazy, Suspense } from "react";
import { shallowEqual, useDispatch, useSelector } from "react-redux";
import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom";

import Avatar from "./components/Avatar";
import Loader from "./components/Loader";
import { fetchCurrentUser } from "./modules/currentUser/currentUser";
import { RootState } from "./rootReducer";
import { AppDispatch } from "./store";

const PUBLIC_ROUTES = ["/privacy", /\/referees\/\d$/];

const PrivacyPolicy = lazy(() => import("./pages/PrivacyPolicy"));
const RefereeProfile = lazy(() => import("./pages/RefereeProfile"));
const Admin = lazy(() => import("./pages/Admin"));
const Test = lazy(() => import("./pages/Test"));
const StartTest = lazy(() => import("./pages/StartTest"));
const NgbProfile = lazy(() => import("./pages/NgbProfile"));
const ImportWizard = lazy(() => import("./pages/ImportWizard"));
const RefereeTests = lazy(() => import("./pages/RefereeTests"));
const Settings = lazy(() => import("./pages/Settings"));

const App = () => {
  const [redirectTo, setRedirectTo] = useState<string>();
  const dispatch = useDispatch<AppDispatch>();
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
  <Suspense fallback={<Loader />}>
    <BrowserRouter>
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
        <Routes>
          <Route  path="/">
            {redirectTo && <Navigate to={redirectTo} replace />}
          </Route>
          <Route
            path="/privacy"
            element={<PrivacyPolicy />}
          />
          <Route
            path="/referees/:id"
            element={<RefereeProfile />}
          />
          <Route
            path="/admin"
            element={<Admin />}
          />
          <Route
            path="/admin/tests/:id"
            element={<Test />}
          />
          <Route
            path="/referees/:refereeId/tests/:testId"
            element={<StartTest />}
          />
          <Route
            path="/national_governing_bodies/:id"
            element={<NgbProfile />}
          />
          <Route
            path="/import/:scope"
            element={<ImportWizard />}
          />
          <Route
            path="/referees/:refereeId/tests"
            element={<RefereeTests />}
          />
          {currentUser?.enabledFeatures.includes("i18n") ? (
            <Route
              path="/settings"
              element={<Settings />}
            />
          ) : null}
        </Routes>
      </div>
    </BrowserRouter>
  </Suspense>
  );
};

export default App;
