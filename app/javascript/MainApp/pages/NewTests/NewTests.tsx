import React, { useState } from 'react'

import TestEditModal from 'MainApp/components/TestEditModal'
import TestsTable from 'MainApp/components/TestsTable'

const NewTests = () => {
  const [isCreateOpen, setIsCreateOpen] = useState(false)

  const handleOpen = () => setIsCreateOpen(true)
  const handleClose = () => setIsCreateOpen(false)

  return (
    <>
      <div className="w-5/6 mx-auto my-8">
        <div className="w-full flex justify-between items-center my-8">
          <h1 className="text-4xl font-extrabold">Certifications</h1>
          <div>
            <button
              className="bg-green text-white rounded py-2 px-4"
              onClick={handleOpen}
            >
              Create Test
            </button>
          </div>
        </div>
        <TestsTable />
      </div>
      {isCreateOpen && <TestEditModal open={isCreateOpen} showClose={true} onClose={handleClose} />}
    </>
  )
}

export default NewTests
