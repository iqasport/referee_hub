import Bugsnag from "@bugsnag/js";
import React, { useEffect, useState, lazy, Suspense } from "react";
import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom";

import Avatar from "./components/Avatar";
import Loader from "./components/Loader";
import { useGetCurrentUserQuery } from "./store/serviceApi";

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
  const { currentData: currentUser, isError } = useGetCurrentUserQuery()
  const roles = currentUser?.roles?.map(r => r.roleType);

  const getRedirect = () => {
    if (roles.includes("IqaAdmin")) return "/admin";
    if (roles.includes("NgbAdmin")) return `/national_governing_bodies/${/*TODO: currentUser?.ownedNgbId*/""}`;
    if (roles.includes("Referee")) return `/referees/me`;

    return null;
  };

  useEffect(() => {
    const isPublic = PUBLIC_ROUTES.some((route) => window.location.pathname.match(route));

    if (isError && !isPublic) {
      window.location.href = `${window.location.origin}/sign_in`;
    }
  }, [isError]);

  useEffect(() => {
    if (currentUser) {
      setRedirectTo(getRedirect());
    }
  }, [currentUser, roles]);

  if (currentUser) Bugsnag.setUser(currentUser.userId);

  if (!currentUser) return <Loader />;

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
            userId={currentUser?.userId}
            ownedNgbId={/* TODO currentUser?.ownedNgbId*/ undefined}
            enabledFeatures={/* TODO currentUser?.enabledFeatures*/ undefined}
            />
        </div>
        <Routes>
          <Route
            path="/"
            element={redirectTo && <Navigate to={redirectTo} replace />}
          />
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
          {/* TODO {currentUser?.enabledFeatures.includes("i18n") ? (
            <Route
              path="/settings"
              element={<Settings />}
            />
          ) : null} */}
        </Routes>
      </div>
    </BrowserRouter>
  </Suspense>
  );
};

export default App;
