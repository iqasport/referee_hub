import BraftEditor, { EditorState } from 'braft-editor';
import classnames from 'classnames'
import { isEqual } from 'lodash';
import React, { useState } from 'react'
import { useDispatch } from 'react-redux';

import { UpdateQuestionRequest } from 'MainApp/apis/question';
import { updateQuestion } from 'MainApp/modules/question/question';
import { GetQuestionsSchemaDatum, Included } from 'MainApp/schemas/getQuestionsSchema';

import RichTextEditor from '../RichTextEditor';
import Answer from './Answer';

interface QuestionProps {
  question: GetQuestionsSchemaDatum;
  index: number;
  answers: Included[];
}

enum ActiveTab {
  Answers = 'answers',
  Details = 'details'
}

const Question = (props: QuestionProps) => {
  const { question, index, answers } = props

  const [activeTab, setActiveTab] = useState<ActiveTab>(ActiveTab.Answers)
  const [isEditing, setIsEditing] = useState(false)
  const [newQuestion, setNewQuestion] = useState<UpdateQuestionRequest>(question.attributes)
  const [newAnswers, setNewAnswers] = useState<Included[]>(answers)
  const [hasChangedQuestion, setHasChangedQuestion] = useState(false)
  const [hasChangedAnswers, setHasChangedAnswers] = useState(false)
  const dispatch = useDispatch()

  const isAnswersActive = activeTab === ActiveTab.Answers
  const isDetailsActive = activeTab === ActiveTab.Details

  const handleTabClick = (newTab: ActiveTab) => () => setActiveTab(newTab)
  const handleEditClick = () => setIsEditing(true)
  const handleEditClose = () => setIsEditing(false)
  const handleQuestionDescriptionChange = (editor: EditorState) => {
    const description = editor.toHTML();
    if (!isEqual(description, newQuestion.description)) {
      setNewQuestion({ ...newQuestion, description })
      if (!hasChangedQuestion) setHasChangedQuestion(true)
    }
  };
  const handleDescriptionChange = (answerId: string, newValue: string) => {
    const newValues = newAnswers.map((answer) => {
      return answer.id === answerId ? { ...answer, description: newValue } : answer 
    })

    if (!hasChangedAnswers) setHasChangedAnswers(true)
    setNewAnswers(newValues)
  }
  const handleCorrectChange = (answerId: string, checked: boolean) => {
    const newValues = newAnswers.map((answer) => {
      if (answerId !== answer.id) {
        if (answer.attributes.correct && checked) {
          return { ...answer, correct: false }
        } else {
          return answer
        }
      }

      return { ...answer, correct: checked }
    })

    setNewAnswers(newValues)
  }

  const handleSave = () => {
    if (hasChangedQuestion) {
      dispatch(updateQuestion(question.id, newQuestion))
    }

    // if (hasChangedAnswers) {
    //   dispatch(updateAnswers(newAnswers))
    // }
  }

  const renderAnswer = (answer: Included) => (
    <Answer
      key={answer.id}
      answer={answer}
      isEditing={isEditing}
      onDescriptionChange={handleDescriptionChange}
      onCorrectChange={handleCorrectChange}
    />
  );

  const renderAddAnswer = () => {
    return <div>add answer</div>
  }

  const renderAnswers = () => {
    return (
      <div>
        <ol className="list-decimal">
          {answers.length ? answers.map(renderAnswer) : renderAddAnswer()}
        </ol>
      </div>
    )
  }

  const renderDetails = () => {
    return (
      <div>
        <div className="my-4">
          <label className="uppercase text-md font-hairline text-gray-400">post-test explanation</label>
          <p>{newQuestion.feedback}</p>
        </div>
        <div className="my-4">
          <label className="uppercase text-md font-hairline text-gray-400">total points</label>
          <p>{newQuestion.pointsAvailable}</p>
        </div>
      </div>
    )
  }

  const renderDescription = () => {
    return isEditing ? (
      <RichTextEditor
        onChange={handleQuestionDescriptionChange}
        content={BraftEditor.createEditorState(newQuestion.description)}
      />
    ) : (
      <div
        dangerouslySetInnerHTML={{ __html: newQuestion.description }}
      />
    );
  }

  return (
    <div className="flex items-start w-full my-4">
      <div className="h-8 w-8 rounded-full bg-green border border-green-darker text-white mr-4 flex items-center justify-center">
        <div>{index}</div>
      </div>
      <div className="w-11/12 border border-gray-300">
        <h4 className="w-full py-2 px-4 border border-gray-400">
          {renderDescription()}
        </h4>
        <div className="flex w-full justify-between mt-4 px-4 min-h-40">
          <div className="w-2/3 px-4">
            {isAnswersActive ? renderAnswers() : renderDetails()}
          </div>
          <div className="flex h-12 flex-wrap">
            <button
              onClick={handleTabClick(ActiveTab.Answers)}
              className={classnames("button-tab", {
                ["active-button-tab"]: isAnswersActive,
              })}
            >
              answers
            </button>
            <button
              onClick={handleTabClick(ActiveTab.Details)}
              className={classnames("button-tab", {
                ["active-button-tab"]: isDetailsActive,
              })}
            >
              explanation
            </button>
            {!isEditing && (
              <button onClick={handleEditClick}>edit question</button>
            )}
            {isEditing && <button onClick={handleEditClose}>cancel</button>}
            {isEditing && <button onClick={handleSave}>save</button>}
          </div>
        </div>
      </div>
    </div>
  );
}

export default Question
