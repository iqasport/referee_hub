import React, { useRef, useState, useEffect, useCallback } from "react";
import { faChevronLeft, faChevronRight } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import TournamentCard from "./TournamentCard";
import { TournamentType } from "../../../store/serviceApi";
import { useNavigate } from "../../../utils/navigationUtils";

const CAROUSEL_BATCH_SIZE = 10;

export type Manager = { id: string; name: string };

export type TournamentData = {
  id: string;
  title: string;
  description: string;
  startDate: string;
  endDate: string;
  type: TournamentType | undefined;
  country: string;
  location: string;
  bannerImageUrl?: string;
  organizer?: string;
  managers?: Manager[];
  isPrivate: boolean;
};

type TournamentSectionProps = {
  tournaments: TournamentData[];
  visibility?: "public" | "private";
  layout?: "grid" | "carousel";
};

const TournamentSection: React.FC<TournamentSectionProps> = ({
  tournaments,
  visibility,
  layout = "grid",
}) => {
  const navigate = useNavigate();
  const carouselRef = useRef<HTMLDivElement>(null);
  const [visibleCount, setVisibleCount] = useState(CAROUSEL_BATCH_SIZE);

  // Reset visible count when tournaments change
  useEffect(() => {
    setVisibleCount(CAROUSEL_BATCH_SIZE);
  }, [tournaments]);

  const loadMore = useCallback(() => {
    setVisibleCount((prev) => Math.min(prev + CAROUSEL_BATCH_SIZE, tournaments.length));
  }, [tournaments.length]);

  // Intersection observer to load more when scrolling near the end
  const loadMoreRef = useRef<HTMLDivElement>(null);
  useEffect(() => {
    if (layout !== "carousel") return undefined;

    const observer = new IntersectionObserver(
      (entries) => {
        if (entries[0].isIntersecting && visibleCount < tournaments.length) {
          loadMore();
        }
      },
      { root: carouselRef.current, rootMargin: "200px", threshold: 0 }
    );

    const currentRef = loadMoreRef.current;
    if (currentRef) {
      observer.observe(currentRef);
    }

    return () => {
      if (currentRef) {
        observer.unobserve(currentRef);
      }
    };
  }, [layout, visibleCount, tournaments.length, loadMore]);

  const scrollCarousel = (direction: "left" | "right") => {
    if (carouselRef.current) {
      const scrollAmount = 340; // Approximate card width + gap
      carouselRef.current.scrollBy({
        left: direction === "left" ? -scrollAmount : scrollAmount,
        behavior: "smooth",
      });
    }
  };

  if (tournaments.length === 0) {
    return (
      <section className="max-w-[80%] mx-auto p-4 text-gray-600">No tournaments available.</section>
    );
  }

  const renderCards = () =>
    tournaments.map((tournament) => {
      const tournamentId = String(tournament.id);
      return (
        <TournamentCard
          key={tournament.id}
          title={tournament.title}
          description={tournament.description}
          startDate={tournament.startDate}
          endDate={tournament.endDate}
          type={tournament.type}
          country={tournament.country}
          location={tournament.location}
          bannerImageUrl={tournament.bannerImageUrl}
          organizer={tournament.organizer}
          onClick={() => {
            navigate(`/tournaments/${tournamentId}`);
          }}
        />
      );
    });

  const visibleTournaments =
    layout === "carousel" ? tournaments.slice(0, visibleCount) : tournaments;

  return (
    <section>
      {visibility && (
        <h2 className="text-2xl font-bold text-gray-900 mb-4">
          {visibility === "public" ? "Public" : "Your"} Tournaments
        </h2>
      )}
      {layout === "carousel" ? (
        <div>
          <div
            ref={carouselRef}
            className="flex gap-6 overflow-x-auto scroll-smooth py-4 scrollbar-hide"
            style={{ scrollbarWidth: "none", msOverflowStyle: "none" }}
          >
            {visibleTournaments.map((tournament) => {
              const tournamentId = String(tournament.id);
              return (
                <div key={tournament.id} className="flex-shrink-0 w-80">
                  <TournamentCard
                    title={tournament.title}
                    description={tournament.description}
                    startDate={tournament.startDate}
                    endDate={tournament.endDate}
                    type={tournament.type}
                    country={tournament.country}
                    location={tournament.location}
                    bannerImageUrl={tournament.bannerImageUrl}
                    organizer={tournament.organizer}
                    onClick={() => {
                      navigate(`/tournaments/${tournamentId}`);
                    }}
                  />
                </div>
              );
            })}
            {/* Invisible trigger element for lazy loading */}
            {visibleCount < tournaments.length && (
              <div
                ref={loadMoreRef}
                className="flex-shrink-0 w-20 flex items-center justify-center"
              >
                <div className="animate-pulse text-gray-400">Loading...</div>
              </div>
            )}
          </div>
          <div className="flex justify-center gap-4 mt-4">
            <button
              onClick={() => scrollCarousel("left")}
              className="bg-white shadow-lg rounded-full p-3 hover:bg-gray-100 transition-colors"
              aria-label="Scroll left"
            >
              <FontAwesomeIcon icon={faChevronLeft} className="text-gray-700" />
            </button>
            <button
              onClick={() => scrollCarousel("right")}
              className="bg-white shadow-lg rounded-full p-3 hover:bg-gray-100 transition-colors"
              aria-label="Scroll right"
            >
              <FontAwesomeIcon icon={faChevronRight} className="text-gray-700" />
            </button>
          </div>
        </div>
      ) : (
        <div className="p-4 grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 mx-auto max-w-[80%]">
          {renderCards()}
        </div>
      )}
    </section>
  );
};

export default TournamentSection;
