import Bugsnag from "@bugsnag/js";
import React, { useEffect, useState, lazy, Suspense } from "react";
import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom";

import { CurrentUserProvider, useCurrentUser } from "./CurrentUserContext";
import Avatar from "./components/Avatar";
import NotificationCenter from "./components/NotificationCenter/NotificationCenter";
import Loader from "./components/Loader";

const PUBLIC_ROUTE_PATTERNS = [/^\/privacy$/, /^\/tournaments$/, /^\/tournaments\/[^/]+$/];

function getErrorStatus(error: unknown): number | undefined {
  if (!error || typeof error !== "object") {
    return undefined;
  }

  const candidate = error as { status?: unknown };
  return typeof candidate.status === "number" ? candidate.status : undefined;
}

const PrivacyPolicy = lazy(() => import("./pages/PrivacyPolicy"));
const RefereeProfile = lazy(() => import("./pages/RefereeProfile"));
const Admin = lazy(() => import("./pages/Admin"));
const Test = lazy(() => import("./pages/Test"));
const StartTest = lazy(() => import("./pages/StartTest"));
const NgbProfile = lazy(() => import("./pages/NgbProfile"));
const ImportWizard = lazy(() => import("./pages/ImportWizard"));
const RefereeTests = lazy(() => import("./pages/RefereeTests"));
const Tournament = lazy(() => import("./pages/Tournaments"));
const TournamentDetails = lazy(() => import("./pages/Tournaments/TournamentId"));
const TeamView = lazy(() => import("./pages/TeamView"));
const TeamManagement = lazy(() => import("./pages/TeamManagement"));

const AppContent = () => {
  const isPublicRoute = PUBLIC_ROUTE_PATTERNS.some((pattern) => pattern.test(window.location.pathname));
  const bugsnagEnabled = Boolean(process.env.BUGSNAG_API_KEY);
  const [redirectTo, setRedirectTo] = useState<string>();
  const { currentUser, error, isError, isLoading } = useCurrentUser();
  const roles = currentUser?.roles?.map((r) => r.roleType).filter((r): r is string => typeof r === "string") ?? [];

  const ownedNgbIds = currentUser
    ? (() => {
        const ngb = (currentUser.roles?.find((r) => r.roleType === "NgbAdmin") as any)?.ngb;
        if (typeof ngb === "string") {
          return [ngb];
        }
        return ngb;
      })()
    : undefined;

  const shouldShowSignInButton = isPublicRoute && !isLoading && !currentUser;

  const getRedirect = () => {
    if (!currentUser) return undefined;
    if (roles.includes("IqaAdmin")) return "/admin";
    if (roles.includes("NgbAdmin")) return `/national_governing_bodies/${ownedNgbIds?.[0]}`;
    if (roles.includes("Referee")) return `/referees/${currentUser.userId}`;

    return undefined;
  };

  useEffect(() => {
    const status = getErrorStatus(error);
    const shouldRedirectToSignIn = status === 401 || status === 403;

    if (!isLoading && isError && shouldRedirectToSignIn && !isPublicRoute) {
      window.location.href = `${window.location.origin}/sign_in`;
    }
  }, [isError, isLoading, isPublicRoute, error]);

  useEffect(() => {
    if (currentUser) {
      setRedirectTo(getRedirect());
    }
  }, [currentUser, roles]);

  useEffect(() => {
    if (bugsnagEnabled && currentUser?.userId) {
      Bugsnag.setUser(currentUser.userId);
    }
  }, [bugsnagEnabled, currentUser?.userId]);

  if (!isPublicRoute && isLoading === true) return <Loader />;

  return (
    <Suspense fallback={<Loader />}>
      <BrowserRouter future={{ v7_startTransition: true, v7_relativeSplatPath: true }}>
        <div>
          <div className="bg-navy-blue text-right text-white py-3 px-10 flex items-center justify-end">
            <p className="flex-shrink mx-8">Management Hub</p>
            {shouldShowSignInButton && (
              <a
                href="/sign_in"
                className="inline-flex items-center px-4 py-2 mr-4 text-sm font-semibold rounded bg-white text-navy-blue hover:bg-gray-100"
              >
                Sign In
              </a>
            )}
            {currentUser && <NotificationCenter currentUserId={currentUser.userId ?? ""} />}
            {currentUser && (
              <Avatar
                firstName={currentUser.firstName ?? ""}
                lastName={currentUser.lastName ?? ""}
                roles={roles}
                userId={currentUser.userId ?? ""}
                ownedNgbId={ownedNgbIds ? ownedNgbIds[0] : undefined}
                enabledFeatures={[]}
              />
            )}
          </div>
          <Routes>
            <Route path="/" element={(redirectTo && <Navigate to={redirectTo} replace />) || <></>} />
            <Route path="/privacy" element={<PrivacyPolicy />} />
            <Route path="/referees/:refereeId" element={<RefereeProfile />} />
            <Route path="/admin" element={<Admin />} />
            <Route path="/admin/tests/:testId" element={<Test />} />
            <Route path="/referees/:refereeId/tests/:testId" element={<StartTest />} />
            <Route path="/national_governing_bodies/:ngbId" element={<NgbProfile />} />
            <Route path="/import/:importScope/:scopeId" element={<ImportWizard />} />
            <Route path="/referees/:refereeId/tests" element={<RefereeTests />} />
            <Route path="/tournaments" element={<Tournament />} />
            <Route path="/tournaments/:tournamentId" element={<TournamentDetails />} />
            <Route path="/teams/:teamId" element={<TeamView />} />
            <Route path="/teams/:teamId/manage" element={<TeamManagement />} />
          </Routes>
        </div>
      </BrowserRouter>
    </Suspense>
  );
};

const App = () => {
  return (
    <CurrentUserProvider>
      <AppContent />
    </CurrentUserProvider>
  );
};

export default App;