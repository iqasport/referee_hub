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
    expect(screen.queryByRole("link", { name: "My Profile" })).not.toBeInTheDocument();
  });

  test.each([
    { content: "My Profile", href: "/referees/123", role: "link" },
    { content: "Logout", href: undefined, role: "button" },
  ])("it makes the entire $content row interactive", ({ content, href, role }) => {
    renderDropdown([{ content, href, onClick: jest.fn() }]);

    const item = screen.getByRole(role, { name: content });
    expect(item).toHaveClass("block", "w-full", "px-4", "py-2", "text-left");
    expect(item.parentElement).not.toHaveClass("px-4", "py-2");
  });

  test("it invokes a button action exactly once and closes the menu", () => {
    const onClick = jest.fn();
    renderDropdown([{ content: "Logout", onClick }]);

    fireEvent.click(screen.getByRole("button", { name: "Logout" }));

    expect(onClick).toHaveBeenCalledTimes(1);
    expect(screen.queryByRole("button", { name: "Logout" })).not.toBeInTheDocument();
  });

  test("it preserves cmd-click navigation without invoking the action or closing the menu", () => {
    const onClick = jest.fn();
    renderDropdown([{ content: "My Profile", href: "/referees/123", onClick }]);

    fireEvent.click(screen.getByRole("link", { name: "My Profile" }), { metaKey: true });

    expect(onClick).not.toHaveBeenCalled();
    expect(screen.getByRole("link", { name: "My Profile" })).toBeInTheDocument();
  });
});