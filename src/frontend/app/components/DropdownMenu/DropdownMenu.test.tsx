import React from "react";
import { fireEvent, render, screen } from "../../utils/test-utils";

import DropdownMenu, { ItemConfig } from "./DropdownMenu";

describe("DropdownMenu", () => {
  const renderDropdown = (items: ItemConfig[]) => {
    render(
      <DropdownMenu
        items={items}
        renderTrigger={(onClick) => (
          <button type="button" onClick={onClick}>
            Open Menu
          </button>
        )}
      />,
    );

    fireEvent.click(screen.getByRole("button", { name: "Open Menu" }));
  };

  beforeEach(() => {
    window.history.pushState({}, "", "/");
  });

  test("it renders href with preserved navigation params", () => {
    window.history.pushState({}, "", "/?impersonate=123&features=isTestFlag");

    renderDropdown([
      {
        content: "My Profile",
        href: "/referees/123",
        onClick: jest.fn(),
      },
    ]);

    const link = screen.getByRole("link", { name: "My Profile" });
    expect(link).toHaveAttribute("href", "/referees/123?impersonate=123&features=isTestFlag");
  });

  test("it does not invoke item onClick on ctrl+click", () => {
    const onClick = jest.fn();

    renderDropdown([
      {
        content: "My Profile",
        href: "/referees/123",
        onClick,
      },
    ]);

    fireEvent.click(screen.getByRole("link", { name: "My Profile" }), { ctrlKey: true });

    expect(onClick).not.toHaveBeenCalled();
  });

  test("it invokes item onClick on regular click", () => {
    const onClick = jest.fn();

    renderDropdown([
      {
        content: "My Profile",
        href: "/referees/123",
        onClick,
      },
    ]);

    fireEvent.click(screen.getByRole("link", { name: "My Profile" }));

    expect(onClick).toHaveBeenCalledTimes(1);
  });
});