import type { ReactNode } from 'react'
import { Form, Modal } from 'antd'
import type { ModalProps } from 'antd'

export type FormModalProps = {
  open: boolean
  loading: boolean
  title: ReactNode
  onClose: () => void
  onSubmit: () => Promise<void>
  children: ReactNode
  width?: number
  okButtonProps?: ModalProps['okButtonProps']
  okText?: string
}

export function FormModal({
  open,
  loading,
  title,
  onClose,
  onSubmit,
  children,
  width,
  okButtonProps,
  okText,
}: FormModalProps) {
  return (
    <Modal
      title={title}
      open={open}
      onCancel={onClose}
      onOk={onSubmit}
      confirmLoading={loading}
      width={width}
      okButtonProps={okButtonProps}
      okText={okText}
    >
      <Form layout="vertical">{children}</Form>
    </Modal>
  )
}
